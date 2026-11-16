//AI
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace InGame.Enemy
{
    /// <summary>
    /// マスク(Pass 0)とアウトライン(Pass 1)を、敵レイヤーのRendererに対して順に描画する
    /// </summary>
    public class EnemyOutlinePass : ScriptableRenderPass
    {
        private const int MaskPassIndex = 0;
        private const int OutlinePassIndex = 1;

        private static readonly List<ShaderTagId> ShaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit")
        };

        private readonly Material _material;
        private readonly LayerMask _layerMask;

        private class PassData
        {
            public RendererListHandle MaskList;
            public RendererListHandle OutlineList;
        }

        public EnemyOutlinePass(Material material, LayerMask layerMask)
        {
            _material = material;
            _layerMask = layerMask;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var lightData = frameData.Get<UniversalLightData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("EnemyOutline", out var passData))
            {
                passData.MaskList = CreateRendererList(renderGraph, renderingData, cameraData, lightData, MaskPassIndex);
                passData.OutlineList = CreateRendererList(renderGraph, renderingData, cameraData, lightData, OutlinePassIndex);

                builder.UseRendererList(passData.MaskList);
                builder.UseRendererList(passData.OutlineList);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.MaskList);
                    context.cmd.DrawRendererList(data.OutlineList);
                });
            }
        }

        private RendererListHandle CreateRendererList(
            RenderGraph renderGraph,
            UniversalRenderingData renderingData,
            UniversalCameraData cameraData,
            UniversalLightData lightData,
            int passIndex)
        {
            var drawingSettings = RenderingUtils.CreateDrawingSettings(
                ShaderTagIds, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
            drawingSettings.overrideMaterial = _material;
            drawingSettings.overrideMaterialPassIndex = passIndex;

            var filteringSettings = new FilteringSettings(RenderQueueRange.all, _layerMask);
            var param = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
            return renderGraph.CreateRendererList(param);
        }
    }
}