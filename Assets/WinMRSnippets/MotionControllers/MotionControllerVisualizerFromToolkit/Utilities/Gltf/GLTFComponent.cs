using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GLTF
{
    class GLTFComponent : MonoBehaviour
    {
            
#pragma warning disable CS0649
        public string Url;
        public Shader GLTFStandard;
        public Shader GLTFConstant;
#pragma warning restore

        public bool Multithreaded = true;
        public int MaximumLod = 300;


        IEnumerator Start()
        {
            UnityWebRequest www = UnityWebRequest.Get(Url);
#if UNITY_5 
            yield return www.Send();
#else
            yield return www.SendWebRequest();
#endif 
            byte[] gltfData = www.downloadHandler.data;

            var loader = new GLTFLoader(
                gltfData,
                gameObject.transform
            );
            loader.SetShaderForMaterialType(GLTFLoader.MaterialType.PbrMetallicRoughness, GLTFStandard);
            loader.SetShaderForMaterialType(GLTFLoader.MaterialType.CommonConstant, GLTFConstant);
            loader.Multithreaded = Multithreaded;
            loader.MaximumLod = MaximumLod;
            yield return loader.Load();
        }
    }
}