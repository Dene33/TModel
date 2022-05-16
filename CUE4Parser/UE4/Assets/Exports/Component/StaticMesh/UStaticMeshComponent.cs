using CUE4Parse.UE4.Assets.Readers;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.Component.StaticMesh
{
    public class UStaticMeshComponent : UObject
    {
        public FStaticMeshComponentLODInfo[]? LODData;
        public ResolvedObject? StaticMesh;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            LODData = Ar.ReadArray(() => new FStaticMeshComponentLODInfo(Ar));
            StaticMesh = GetOrDefault<ResolvedObject>(nameof(StaticMesh));
        }

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);

            if (LODData is { Length: <= 0 }) return;
            writer.WritePropertyName("LODData");
            serializer.Serialize(writer, LODData);
        }
    }
}