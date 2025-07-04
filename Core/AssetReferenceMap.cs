﻿using System;
using System.Collections.Generic;
using System.Text;
using AssetCatalogue.Nodes;

namespace AssetCatalogue
{
    public class AssetReferenceMap
    {
        public readonly string assetName;
        public Dictionary<long, TransformNode> goID2Transform;
        public Dictionary<long, GameObjectNode> goID2GameObject;
        public Dictionary<string, List<ComponentNode>> assetGuid2Component;
        public Dictionary<long, TransformNode> tsfmID2Transform;

        public AssetReferenceMap(string assetName)
        {
            this.assetName = assetName;
            goID2Transform = new Dictionary<long, TransformNode> { };
            goID2GameObject = new Dictionary<long, GameObjectNode> { };
            assetGuid2Component = new Dictionary<string, List<ComponentNode>> { };
            tsfmID2Transform = new Dictionary<long, TransformNode> { };
        }
    
        public bool TryCreateCatalogueEntry(string guid)
        {
            if (!ContainsAsset(guid))
            {
                return false;
            }
        
            var foundAssets = assetGuid2Component[guid];
            for (var i = 0; i < foundAssets.Count; i++)
            {
                var component = foundAssets[i];
                var foundRecord = component.Assets.FindAll(refr => refr.Guid == guid);
                if (foundRecord.Count > 1)
                {
                    AssetParasite.Logger.WriteLine(
                        $"WARNING: Found {foundRecord.Count} refs for asset {guid} on {component.ComponentType} (of {goID2GameObject[component.GameObjectFileID].Name})");
                    i += foundRecord.Count - 1;
                }

                if (foundRecord.Count == 0)
                {
                    AssetParasite.Logger.WriteLine($"WARNING: There are no assets for GUID {guid} in scene {assetName}");
                    return false;
                }

                var componentIdx = ResolveComponentData(foundRecord[0], component);
                var hierarchyPath = ResolveHierarchyPath(component.GameObjectFileID);

                CatalogueGenerator.Writer.InsertAsset(AssetParasite.Config.ModGuid, assetName, guid, hierarchyPath.goName, hierarchyPath.tsfmPath,
                    componentIdx);

                if (!AssetParasite.Config.IncludeAllRefs) return true;
            }

            return true;
        }

        public bool ContainsAsset(string assetGuid)
        {
            return assetGuid2Component.ContainsKey(assetGuid);
        }

        private (string goName, string tsfmPath) ResolveHierarchyPath(long goID)
        {
            if (goID == 0) return ("", "");
            StringBuilder sb = new StringBuilder();
            TransformNode currNode = goID2Transform[goID];

            while (currNode.Parent != 0)
            {
                if (sb.Length >= 1) sb.Insert(0, ';');
                sb.Insert(0, $"{currNode.RootOrder}");
                currNode = tsfmID2Transform[currNode.Parent];
            }
             
            if (AssetParasite.Config.VerboseLogging) AssetParasite.Logger.WriteLine($"Resolving GameObject @ {goID}");

            var rootGO = goID2GameObject[currNode.GameObjectID];

            return (rootGO.Name, sb.ToString());
        }

        private long ResolveComponentData(ComponentNode.AssetReference assetRef, ComponentNode component)
        {
            long propId;
            if (component is MonoBehaviourNode scriptNode)
            {
                propId = CatalogueGenerator.Writer.InsertFieldData(scriptNode.ComponentType, assetRef.FieldName, assetRef.IsCollection, assetRef.CollectionIndex, scriptNode.ScriptGUID);
            }
            else
            {
                propId = CatalogueGenerator.Writer.InsertFieldData(component.ComponentType, assetRef.FieldName, assetRef.IsCollection, assetRef.CollectionIndex);
            }
        
            return propId;
        }

    }
}