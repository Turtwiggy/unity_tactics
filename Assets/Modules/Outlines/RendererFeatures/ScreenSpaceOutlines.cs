using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

// RenderTargetHandle to RTHandle
//
// 1 
// RenderTargetHandle renderTarget;
// (new) RTHandle renderTarget;
//
// 2 Associate the shader property with the render target using RTHandles.Alloc instead of renderTarget.Init
// renderTarget.Init("_ShaderProperty");
// (new) renderTarget = RTHandles.Alloc("_ShaderProperty", name: "_ShaderProperty");
//
// 3 Assign temporary render texture using the shader property identifier
// cmd.GetTemporaryRT(renderTarget.id, targetDescriptor, filterMode);
// (new) cmd.GetTemporaryRT(Shader.PropertyToID(renderTarget.name), targetDescriptor, filterMode);
//
// 4 Modify ConfigureTarget to use the new RTHandle instead of the old RenderTargetIdentifier
// ConfigureTarget(renderTarget.id);
// (new) ConfigureTarget(renderTarget);
//
// 5 Get the render texture identifier from the shader property
// cmd.ReleaseTemporaryRT(renderTarget.id);
// (new) cmd.ReleaseTemporaryRT(Shader.PropertyToID(renderTarget.name));

namespace Wiggy
{
  // https://github.com/Robinseibold/Unity-URP-Outlines/tree/main

  public class ScreenSpaceOutlines : ScriptableRendererFeature
  {
    [System.Serializable]
    private class ScreenSpaceOutlineSettings
    {
      [Header("General Outline Settings")]
      public Color outline_color = Color.black;
      [Range(0.0f, 20.0f)]
      public float outline_scale = 1.0f;

      [Header("Depth Settings")]
      [Range(0.0f, 100.0f)]
      public float depth_threshold = 1.5f;
      [Range(0.0f, 500.0f)]
      public float roberts_crossMultiplier = 100.0f;

      [Header("Normal Settings")]
      [Range(0.0f, 1.0f)]
      public float normal_threshold = 0.4f;

      [Header("Depth Normal Relation Settings")]
      [Range(0.0f, 2.0f)]
      public float steep_angle_threshold = 0.2f;
      [Range(0.0f, 500.0f)]
      public float steep_angle_multiplier = 25.0f;
    }

    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings
    {
      [Header("General Scene View Space Normal Texture Settings")]
      public RenderTextureFormat color_format;
      public int depth_buffer_bits = 16;
      public FilterMode filter_mode;
      public Color background_color = Color.black;

      [Header("View Space Normal Texture Object Draw Settings")]
      public PerObjectData per_object_data;
      public bool enable_dynamic_batching;
      public bool enable_instancing;
    }

    private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
      private ViewSpaceNormalsTextureSettings normals_texture_settings;
      private FilteringSettings filtering_settings;
      private FilteringSettings occluder_filtering_settings;
      private readonly List<ShaderTagId> shader_tag_id_list;
      private readonly Material occluders_material;
      private readonly Material normals_material;
      private readonly RTHandle normals;

      public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent, LayerMask layerMask, LayerMask occluderLayerMask, ViewSpaceNormalsTextureSettings settings)
      {
        this.renderPassEvent = renderPassEvent;
        this.normals_texture_settings = settings;
        this.filtering_settings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
        this.occluder_filtering_settings = new FilteringSettings(RenderQueueRange.opaque, occluderLayerMask);

        shader_tag_id_list = new List<ShaderTagId>{
          new ShaderTagId("UniversalForward"),
          new ShaderTagId("UniversalForwardOnly"),
          new ShaderTagId("LightweightForward"),
          new ShaderTagId("SRPDefaultUnlit"),
        };

        normals = RTHandles.Alloc("_SceneViewSpaceNormals", name: "_SceneViewSpaceNormals");
        normals_material = new Material(Shader.Find("Hidden/ViewSpaceNormals"));

        occluders_material = new Material(Shader.Find("Hidden/UnlitColor"));
        occluders_material.SetColor("_Color", normals_texture_settings.background_color);
      }

      public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
      {
        base.Configure(cmd, cameraTextureDescriptor);

        RenderTextureDescriptor normals_texture_descriptor = cameraTextureDescriptor;
        normals_texture_descriptor.colorFormat = normals_texture_settings.color_format;
        normals_texture_descriptor.depthBufferBits = normals_texture_settings.depth_buffer_bits;
        cmd.GetTemporaryRT(Shader.PropertyToID(normals.name), normals_texture_descriptor, FilterMode.Point);

        ConfigureTarget(normals);
        ConfigureClear(ClearFlag.All, normals_texture_settings.background_color);
      }

      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        if (!normals_material || !occluders_material)
          return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
        {
          context.ExecuteCommandBuffer(cmd);
          cmd.Clear();

          DrawingSettings draw_settings = CreateDrawingSettings(shader_tag_id_list, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
          draw_settings.perObjectData = normals_texture_settings.per_object_data;
          draw_settings.enableDynamicBatching = normals_texture_settings.enable_dynamic_batching;
          draw_settings.enableInstancing = normals_texture_settings.enable_instancing;
          draw_settings.overrideMaterial = normals_material;

          DrawingSettings occluder_settings = draw_settings;
          occluder_settings.overrideMaterial = occluders_material;

          context.DrawRenderers(renderingData.cullResults, ref draw_settings, ref filtering_settings);
          context.DrawRenderers(renderingData.cullResults, ref occluder_settings, ref occluder_filtering_settings);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
      }

      public override void OnCameraCleanup(CommandBuffer cmd)
      {
        cmd.ReleaseTemporaryRT(Shader.PropertyToID(normals.name));
      }
    }

    private class ScreenSpaceOutlinePass : ScriptableRenderPass
    {
      private readonly Material screen_space_outline_material;

      RenderTargetIdentifier camera_color_target;
      RenderTargetIdentifier temporaryBuffer;
      int temporaryBufferID = Shader.PropertyToID("_TemporaryBuffer");

      public ScreenSpaceOutlinePass(RenderPassEvent evt, ScreenSpaceOutlineSettings settings)
      {
        this.renderPassEvent = evt;
        screen_space_outline_material = new Material(Shader.Find("Hidden/Outlines"));
        screen_space_outline_material.SetColor("_OutlineColor", settings.outline_color);
        screen_space_outline_material.SetFloat("_OutlineScale", settings.outline_scale);
        screen_space_outline_material.SetFloat("_DepthThreshold", settings.depth_threshold);
        screen_space_outline_material.SetFloat("_RobertsCrossMultiplier", settings.roberts_crossMultiplier);
        screen_space_outline_material.SetFloat("_NormalThreshold", settings.normal_threshold);
        screen_space_outline_material.SetFloat("_SteepAngleThreshold", settings.steep_angle_threshold);
        screen_space_outline_material.SetFloat("_SteepAngleMultiplier", settings.steep_angle_multiplier);
      }

      public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
      {
        RenderTextureDescriptor temporaryTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        temporaryTargetDescriptor.depthBufferBits = 0;

        cmd.GetTemporaryRT(temporaryBufferID, temporaryTargetDescriptor, FilterMode.Bilinear);
        temporaryBuffer = new RenderTargetIdentifier(temporaryBufferID);

        camera_color_target = renderingData.cameraData.renderer.cameraColorTarget;
      }

      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        if (!screen_space_outline_material)
          return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines")))
        {
          Blit(cmd, camera_color_target, temporaryBuffer);
          Blit(cmd, temporaryBuffer, camera_color_target, screen_space_outline_material);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
      }

      public override void OnCameraCleanup(CommandBuffer cmd)
      {
        cmd.ReleaseTemporaryRT(temporaryBufferID);
      }

    }

    [SerializeField] private RenderPassEvent render_pass_event = RenderPassEvent.AfterRenderingOpaques;
    [SerializeField] private LayerMask outlines_layer_mask;
    [SerializeField] private LayerMask outlines_occluder_layer_mask;

    [SerializeField] private ScreenSpaceOutlineSettings outline_settings = new();
    [SerializeField] private ViewSpaceNormalsTextureSettings view_space_normals_texture_settings = new();

    private ViewSpaceNormalsTexturePass view_space_normals_texture_pass;
    private ScreenSpaceOutlinePass screen_space_outline_pass;

    public override void Create()
    {
      if (render_pass_event < RenderPassEvent.BeforeRenderingPrePasses)
        render_pass_event = RenderPassEvent.BeforeRenderingPrePasses;

      view_space_normals_texture_pass = new ViewSpaceNormalsTexturePass(render_pass_event, outlines_layer_mask, outlines_occluder_layer_mask, view_space_normals_texture_settings);
      screen_space_outline_pass = new ScreenSpaceOutlinePass(render_pass_event, outline_settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
      renderer.EnqueuePass(view_space_normals_texture_pass);
      renderer.EnqueuePass(screen_space_outline_pass);
    }
  }
}
