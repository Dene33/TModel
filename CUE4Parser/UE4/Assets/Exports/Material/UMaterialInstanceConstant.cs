using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.UObject;

namespace CUE4Parse.UE4.Assets.Exports.Material
{
    public class UMaterialInstanceConstant : UMaterialInstance
    {
        public Dictionary<string, float> ScalarParameterValues { get; } = new();
        public Dictionary<string, FPackageIndex> TextureParameterValues { get; } = new();
        public Dictionary<string, FLinearColor> VectorParameterValues { get; } = new();

        public new FMaterialInstanceBasePropertyOverrides? BasePropertyOverrides;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            FScalarParameterValue[] _ScalarParameterValues = GetOrDefault(nameof(ScalarParameterValues), Array.Empty<FScalarParameterValue>());
            foreach (var scaler in _ScalarParameterValues)
            {
                ScalarParameterValues.Add(scaler.Name, scaler.ParameterValue);
            }
            FTextureParameterValue[] _TextureParameterValues = GetOrDefault(nameof(TextureParameterValues), Array.Empty<FTextureParameterValue>());
            foreach (var texture in _TextureParameterValues)
            {
                TextureParameterValues.Add(texture.Name, texture.ParameterValue);
            }
            FVectorParameterValue[] _VectorParameterValues = GetOrDefault(nameof(VectorParameterValues), Array.Empty<FVectorParameterValue>());
            foreach (var vector in _VectorParameterValues)
            {
                VectorParameterValues.Add(vector.Name, vector.ParameterValue);
            }
            BasePropertyOverrides = GetOrDefault<FMaterialInstanceBasePropertyOverrides>(nameof(BasePropertyOverrides));
        }
    }
}
