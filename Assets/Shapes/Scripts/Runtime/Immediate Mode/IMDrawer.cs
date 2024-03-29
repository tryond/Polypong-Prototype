﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	struct IMDrawer : IDisposable {

		internal static MetaMpb metaMpbPrevious;
		static Dictionary<Material, string[]> matKeywords = new Dictionary<Material, string[]>();

		static string[] GetMaterialKeywords( Material m ) {
			if( matKeywords.TryGetValue( m, out string[] kws ) == false )
				matKeywords[m] = kws = m.shaderKeywords;
			return kws;
		}

		MetaMpb metaMpb;
		ShapeDrawState drawState;
		Matrix4x4 mtx;

		public IMDrawer( MetaMpb metaMpb, Material sourceMat, Mesh sourceMesh, int submesh = 0, bool cachedTMP = false ) {
			this.mtx = Draw.Matrix;
			this.metaMpb = metaMpb;

			#if UNITY_EDITOR
			if( sourceMat == null )
				Debug.Log( "Input material is null :(" );
			#endif
			if( DrawCommand.IsAddingDrawCommandsToBuffer ) {
				Draw.style.renderState.shader = sourceMat.shader;
				Draw.style.renderState.keywords = GetMaterialKeywords( sourceMat );

				if( cachedTMP ) {
					// instantiate the mesh and then delete it after this DrawCommand has been executed
					drawState.mesh = Object.Instantiate( sourceMesh );
					drawState.mat = Object.Instantiate( sourceMat );
					ApplyGlobalPropertiesTMP( drawState.mat ); // a lil gross but sfine
					List<Object> cache = DrawCommand.CurrentWritingCommandBuffer.cachedAssets;
					cache.Add( drawState.mesh );
					cache.Add( drawState.mat );
				} else {
					drawState.mat = IMMaterialPool.GetMaterial( ref Draw.style.renderState );
					drawState.mesh = sourceMesh;
				}

				drawState.submesh = submesh;

				// did we switch mpb?
				// this means we definitely can't merge with the previous call, finalize the prev one
				if( metaMpbPrevious != metaMpb && metaMpbPrevious != null && metaMpbPrevious.HasContent )
					DrawCommand.CurrentWritingCommandBuffer.drawCalls.Add( metaMpbPrevious.ExtractDrawCall() ); // finalize previous buffer

				// see if we can merge with the current mpb (which may or may not be equal to prevMpb)
				if( metaMpb.PreAppendCheck( drawState, mtx ) == false ) {
					// we can't append it for whatever reason
					DrawCommand.CurrentWritingCommandBuffer.drawCalls.Add( metaMpb.ExtractDrawCall() ); // finalize previous buffer
					if( metaMpb.PreAppendCheck( drawState, mtx ) == false ) // append again now that the call has been dispatched
						Debug.LogWarning( "MetaMpb somehow not ready to be initialized" ); // really should never happen
				}

				metaMpbPrevious = metaMpb;
			} else {
				drawState.mesh = sourceMesh;
				drawState.mat = sourceMat;
				drawState.submesh = submesh;
				if( metaMpb.PreAppendCheck( drawState, mtx ) == false )
					Debug.LogError( "Somehow PreAppendCheck failed for this draw" );
				ApplyGlobalProperties(); // this will set render state of the material. todo: will this modify the assets? this seems bad
			}
		}

		void ApplyGlobalProperties() {
			if( DrawCommand.IsAddingDrawCommandsToBuffer == false ) { // mpbs can't carry render state
				drawState.mat.SetFloat( ShapesMaterialUtils.propZTest, (float)Draw.ZTest );
				drawState.mat.SetFloat( ShapesMaterialUtils.propZOffsetFactor, Draw.ZOffsetFactor );
				drawState.mat.SetFloat( ShapesMaterialUtils.propZOffsetUnits, Draw.ZOffsetUnits );
				drawState.mat.SetFloat( ShapesMaterialUtils.propStencilComp, (float)Draw.StencilComp );
				drawState.mat.SetFloat( ShapesMaterialUtils.propStencilOpPass, (float)Draw.StencilOpPass );
				drawState.mat.SetFloat( ShapesMaterialUtils.propStencilID, Draw.StencilRefID );
				drawState.mat.SetFloat( ShapesMaterialUtils.propStencilReadMask, Draw.StencilReadMask );
				drawState.mat.SetFloat( ShapesMaterialUtils.propStencilWriteMask, Draw.StencilWriteMask );
			}
		}

		// this is a little gross because it's duplicated, kinda, but we have to deal with gross things sometimes
		static void ApplyGlobalPropertiesTMP( Material m ) {
			m.SetInt( ShapesMaterialUtils.propZTestTMP, (int)Draw.ZTest );
			// m.SetFloat( ShapesMaterialUtils.propZOffsetFactor, Draw.ZOffsetFactor ); // not supported by TMP shaders
			// m.SetInt( ShapesMaterialUtils.propZOffsetUnits, Draw.ZOffsetUnits ); // not supported by TMP shaders
			m.SetInt( ShapesMaterialUtils.propStencilComp, (int)Draw.StencilComp );
			m.SetInt( ShapesMaterialUtils.propStencilOpPass, (int)Draw.StencilOpPass );
			m.SetInt( ShapesMaterialUtils.propStencilIDTMP, Draw.StencilRefID );
			m.SetInt( ShapesMaterialUtils.propStencilReadMask, Draw.StencilReadMask );
			m.SetInt( ShapesMaterialUtils.propStencilWriteMask, Draw.StencilWriteMask );
		}

		public void Dispose() {
			if( DrawCommand.IsAddingDrawCommandsToBuffer == false ) {
				// we're in direct draw mode
				metaMpb.ApplyDirectlyToMaterial();
				drawState.mat.SetPass( 0 );
				Graphics.DrawMeshNow( drawState.mesh, mtx, drawState.submesh );
			} else if( ShapesConfig.Instance.useImmediateModeInstancing == false ) {
				// finalize the draw if we're not using instancing
				DrawCommand.CurrentWritingCommandBuffer.drawCalls.Add( metaMpb.ExtractDrawCall() );
			}
		}

	}

}