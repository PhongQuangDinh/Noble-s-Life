using NeoModLoader.api;
using UnityEngine;

namespace PowerBox {
  public class OutdatedNml : MonoBehaviour, IMod {
    private ModDeclare _modDeclare;
    private GameObject _gameObject;
    public ModDeclare GetDeclaration() {
      return _modDeclare;
    }
    public GameObject GetGameObject() {
      return _gameObject;
    }
    public string GetUrl() {
      return "https://github.com/PhongQuangDinh/Noble-s-Life";
    }
    private const string ERROR_MESSAGE = "You're using an outdated NML version that doesn't support precompiled mods. Please get version 1.0.9 of NeoModLoader or later.";
    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject) {
      _modDeclare = pModDecl;
      _gameObject = pGameObject;
      Debug.LogError(ERROR_MESSAGE);
    }
    private byte _updateCounter;
    public void Update() {
      if (_updateCounter++ == 120) {
        Debug.LogError(ERROR_MESSAGE);
        try {
          TryDisplayUpdatePopup();
        } catch (System.Exception) {
          // this is fine
        }
      }
    }
    public void TryDisplayUpdatePopup() {
      if (Config.gameLoaded) {
        WorldTip.showNow(ERROR_MESSAGE, false, "top");
      }
    }
  }
}
