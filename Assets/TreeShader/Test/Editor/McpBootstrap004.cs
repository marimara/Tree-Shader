#if UNITY_EDITOR
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class McpBootstrap004
{
    static McpBootstrap004()
    {
        EditorApplication.delayCall += Connect;
    }

    private static async void Connect()
    {
        var configuration = EditorConfigurationCache.Instance;
        configuration.SetUseHttpTransport(true);
        configuration.SetHttpTransportScope("local");
        configuration.SetHttpBaseUrl("http://127.0.0.1:8080");

        bool connected = await MCPServiceLocator.Bridge.StartAsync();
        Debug.Log($"[TreeShader Spec 004] Temporary MCP bootstrap connected: {connected}");
    }
}
#endif
