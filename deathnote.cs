using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ItemStatsSystem;
using UnityEngine;
using Duckov;

namespace deathnote
{
    // Duckov 모드 로더 엔트리
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        protected override void OnAfterSetup()
        {
            try
            {
                GameObject root = new GameObject("DeathNoteRoot");
                UnityEngine.Object.DontDestroyOnLoad(root);
                root.AddComponent<DeathNoteManager>();
                Debug.Log("[DeathNote] OnAfterSetup - Manager init OK");
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] OnAfterSetup 예외: " + ex);
            }
        }
    }

    public class DeathNoteManager : MonoBehaviour
    {
        private const KeyCode TOGGLE_KEY = KeyCode.Insert;
        private const float KILL_DELAY = 10f;

        private static readonly BindingFlags BINDING_FLAGS =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        // ───── UI 언어 ─────
        private enum DeathNoteUILang
        {
            Korean,
            Japanese,
            English
        }

        private DeathNoteUILang GetUILang()
        {
            SystemLanguage lang = Application.systemLanguage;

            if (lang == SystemLanguage.Korean)
                return DeathNoteUILang.Korean;

            if (lang == SystemLanguage.Japanese)
                return DeathNoteUILang.Japanese;

            return DeathNoteUILang.English;
        }

        private string GetUITitleText()
        {
            switch (GetUILang())
            {
                case DeathNoteUILang.Japanese:
                    return "このノートに名前を書かれた者は10秒後に死ぬ。";
                case DeathNoteUILang.English:
                    return "Anyone whose name is written in this note will die in 10 seconds.";
                default:
                    return "이 노트에 이름이 적힌 존재는 10초 후에 죽는다.";
            }
        }

        private string GetUIDescriptionText()
        {
            switch (GetUILang())
            {
                case DeathNoteUILang.Japanese:
                    return "ボスの名前を正確に入力して「作成」を押してください.";
                case DeathNoteUILang.English:
                    return "Enter the boss name exactly and press Write.";
                default:
                    // 한국어는 아래 Literal을 그대로 쓸 거라 여기서는 사용 안 함
                    return "보스 이름을 정확히 입력하고 작성을 누르세요.";
            }
        }

        private string GetUIFooterText()
        {
            switch (GetUILang())
            {
                case DeathNoteUILang.Japanese:
                    return "Insert: ノートを開く/閉じる   /   Enter: 作成   /   Esc: 閉じる";
                case DeathNoteUILang.English:
                    return "Insert: Open/Close note   /   Enter: Write   /   Esc: Close";
                default:
                    return "Insert: 노트 열기/닫기   /   Enter: 작성   /   Esc: 닫기";
            }
        }

        private string GetUIButtonText()
        {
            switch (GetUILang())
            {
                case DeathNoteUILang.Japanese:
                    return "作成";
                case DeathNoteUILang.English:
                    return "Write";
                default:
                    return "작성";
            }
        }

        // ───── 보스 이름 매핑 ─────
        private class BossEntry
        {
            public string Input;
            public string Search;
        }

        private static readonly BossEntry[] BossEntries = new BossEntry[]
        {
            new BossEntry { Input = "로든",           Search = "로든" },
            new BossEntry { Input = "광산장",         Search = "광산장" },
            new BossEntry { Input = "BA 대장",        Search = "BA 대장" },
            new BossEntry { Input = "파리 대장",      Search = "파리 대장" },
            new BossEntry { Input = "축구 주장",      Search = "축구 주장" },
            new BossEntry { Input = "폭주 아케이드",  Search = "폭주 아케이드" },
            new BossEntry { Input = "폭주 기계 거미", Search = "폭주 기계 거미" },
            new BossEntry { Input = "???",            Search = "???"},
            new BossEntry { Input = "꼬마덕",         Search = "꼬마덕" },
            new BossEntry { Input = "비다",           Search = "비다" },
            new BossEntry { Input = "쓰리샷 형님",    Search = "쓰리샷 형님" },
            new BossEntry { Input = "폭탄광",         Search = "폭탄광" },
            new BossEntry { Input = "바리케이드",     Search = "바리케이드" },
            new BossEntry { Input = "미셀",           Search = "미셀" },
            new BossEntry { Input = "고급 엔지니어",  Search = "고급 엔지니어" },
            new BossEntry { Input = "샷건",           Search = "샷건" },
            new BossEntry { Input = "푸룽푸룽",       Search = "푸룽푸룽" },
            new BossEntry { Input = "구루구루",       Search = "구루구루" },
            new BossEntry { Input = "팔라팔라",       Search = "팔라팔라" },
            new BossEntry { Input = "빌리빌리",       Search = "빌리빌리" },
            new BossEntry { Input = "코코코코",       Search = "코코코코" },
            new BossEntry { Input = "흥이",           Search = "흥이" },
            new BossEntry { Input = "교도관",         Search = "교도관" },
            new BossEntry { Input = "폭풍?",          Search = "폭풍?" },
            new BossEntry { Input = "일진",           Search = "일진" },
            new BossEntry { Input = "급속 단장",      Search = "급속 단장" },
            new BossEntry { Input = "방랑자",         Search = "방랑자" },
            new BossEntry { Input = "라이트맨",       Search = "라이트맨" },

            new BossEntry { Input = "Pato Chapo",             Search = "Pato Chapo" },
            new BossEntry { Input = "Man of Light",           Search = "Man of Light" },
            new BossEntry { Input = "Speedy Group Commander", Search = "Speedy Group Commander" },
            new BossEntry { Input = "Lordon",                 Search = "Lordon" },
            new BossEntry { Input = "Vida",                   Search = "Vida" },
            new BossEntry { Input = "Big Xing",               Search = "Big Xing" },
            new BossEntry { Input = "Rampaging Arcade",       Search = "Rampaging Arcade" },
            new BossEntry { Input = "Senior Engineer",        Search = "Senior Engineer" },
            new BossEntry { Input = "Triple-Shot Man",        Search = "Triple-Shot Man" },
            new BossEntry { Input = "Misel",                  Search = "Misel" },
            new BossEntry { Input = "Mine Manager",           Search = "Mine Manager" },
            new BossEntry { Input = "Shotgunner",             Search = "Shotgunner" },
            new BossEntry { Input = "Mad Bomber",             Search = "Mad Bomber" },
            new BossEntry { Input = "Security Captain",       Search = "Security Captain" },
            new BossEntry { Input = "Fly Captain",            Search = "Fly Captain" },
            new BossEntry { Input = "School Bully",           Search = "School Bully" },
            new BossEntry { Input = "Billy Billy",            Search = "Billy Billy" },
            new BossEntry { Input = "Gulu Gulu",              Search = "Gulu Gulu" },
            new BossEntry { Input = "Pala Pala",              Search = "Pala Pala" },
            new BossEntry { Input = "Pulu Pulu",              Search = "Pulu Pulu" },
            new BossEntry { Input = "Koko Koko",              Search = "Koko Koko" },
            new BossEntry { Input = "Roadblock",              Search = "Roadblock" },

            new BossEntry { Input = "チビガモ",          Search = "チビガモ" },
            new BossEntry { Input = "光の男",           Search = "光の男" },
            new BossEntry { Input = "ロードン",         Search = "ロードン" },
            new BossEntry { Input = "スピード団団長",    Search = "スピード団団長" },
            new BossEntry { Input = "ハエ隊長",         Search = "ハエ隊長" },
            new BossEntry { Input = "暴走アーケード",    Search = "暴走アーケード" },
            new BossEntry { Input = "ヴィーダ",         Search = "ヴィーダ" },
            new BossEntry { Input = "いじめっ子",       Search = "いじめっ子" },
            new BossEntry { Input = "施設長",           Search = "施設長" },
            new BossEntry { Input = "マルセル",         Search = "マルセル" },
            new BossEntry { Input = "上級エンジニア",    Search = "上級エンジニア" },
            new BossEntry { Input = "トリプルS親分",    Search = "トリプルS親分" },
            new BossEntry { Input = "ショットガンナー",  Search = "ショットガンナー" },
            new BossEntry { Input = "BA隊長",           Search = "BA隊長" },
            new BossEntry { Input = "ロードブロック",    Search = "ロードブロック" },
            new BossEntry { Input = "グルグル",         Search = "グルグル" },
            new BossEntry { Input = "パラパラ",         Search = "パラパラ" },
            new BossEntry { Input = "ビッグシン",       Search = "ビッグシン" },
            new BossEntry { Input = "ビリビリ",         Search = "ビリビリ" },
            new BossEntry { Input = "プロプロ",         Search = "プロプロ" },
            new BossEntry { Input = "ロロロロ",         Search = "ロロロロ" },
            new BossEntry { Input = "爆弾マニア",       Search = "爆弾マニア" },
            new BossEntry { Input = "看守長",           Search = "看守長" },
            new BossEntry { Input = "レイダー",         Search = "レイダー" },
        };

        // ───── UI / 상태 ─────
        private bool _uiVisible;
        private bool _uiActive;
        private Rect _windowRect;
        private string _inputName = string.Empty;

        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _hudLabelStyle;
        private GUIStyle _textFieldStyle;
        private GUIStyle _buttonStyle;

        private GUIStyle _noteOuterStyle;
        private GUIStyle _noteInnerStyle;

        private GUIStyle _bubbleBoxStyle;
        private GUIStyle _bubbleTextStyle;

        private GUIStyle _cursorStyle;   // 가짜 커서용 스타일 (▲, 빨간색)

        private Texture2D _texBlack;
        private Texture2D _texWhite;
        private bool _stylesReady;
        private bool _justOpened;

        // 말풍선 해골
        private Texture2D _skullTexture;
        private bool _skullSearched;

        // 입력/카메라 차단용
        private MonoBehaviour _charInput;
        private MonoBehaviour _playerInput;
        private MonoBehaviour _cameraController;
        private CursorManager _cursorManager;

        // 예약된 죽음
        private Component _pendingTarget;
        private float _pendingTimer;
        private string _pendingName;

        // 이미 죽인 보스 Health 유지
        private readonly List<Health> _deadLockList = new List<Health>();

        private void Awake()
        {
            _windowRect = new Rect(
                Screen.width * 0.5f - 220f,
                Screen.height * 0.5f - 170f,
                440f,
                340f
            );

            Debug.Log("[DeathNote] Manager Awake");
        }

        private void Update()
        {
            if (Input.GetKeyDown(TOGGLE_KEY))
            {
                ToggleUI();
            }

            if (_uiVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseUI();
            }

            if (_pendingTarget != null)
            {
                _pendingTimer -= Time.deltaTime;
                if (_pendingTimer <= 0f)
                {
                    ExecuteDeath(_pendingTarget, _pendingName);
                    _pendingTarget = null;
                    _pendingName = null;
                }
            }
        }

        private void LateUpdate()
        {
            // 노트 열려 있을 때는 항상 커서 잠금 해제 + 진짜 커서는 숨김 (가짜 커서만 그림)
            if (_uiActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
            }

            // 죽인 보스 HP 다시 차지 않게 매 프레임 잠금
            if (_deadLockList.Count > 0)
            {
                for (int i = _deadLockList.Count - 1; i >= 0; i--)
                {
                    Health h = _deadLockList[i];
                    if (h == null)
                    {
                        _deadLockList.RemoveAt(i);
                        continue;
                    }

                    EnsureHealthStaysDead(h);
                }
            }
        }

        // ───── UI 토글 ─────
        private void ToggleUI()
        {
            if (_uiVisible)
                CloseUI();
            else
                OpenUI();
        }

        private void OpenUI()
        {
            _uiVisible = true;
            _inputName = string.Empty;
            _justOpened = true;
            EnterUIMode();
        }

        private void CloseUI()
        {
            _uiVisible = false;
            _justOpened = false;
            ExitUIMode();
        }

        private void EnterUIMode()
        {
            if (_uiActive)
                return;

            _uiActive = true;

            try
            {
                MonoBehaviour[] all = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                if (all != null && all.Length > 0)
                {
                    for (int i = 0; i < all.Length; i++)
                    {
                        MonoBehaviour mb = all[i];
                        if (mb == null) continue;

                        Type t = mb.GetType();
                        string name = t.Name;

                        if (_charInput == null && name == "CharacterInputControl")
                        {
                            _charInput = mb;
                            _charInput.enabled = false;
                            Debug.Log("[DeathNote] CharacterInputControl disabled.");
                            continue;
                        }

                        if (_playerInput == null && name == "PlayerInput")
                        {
                            _playerInput = mb;
                            try
                            {
                                MethodInfo mDeactivate = t.GetMethod("DeactivateInput", BINDING_FLAGS);
                                if (mDeactivate != null)
                                {
                                    mDeactivate.Invoke(_playerInput, null);
                                    Debug.Log("[DeathNote] PlayerInput DeactivateInput() 호출 (game input blocked).");
                                }
                                else
                                {
                                    _playerInput.enabled = false;
                                    Debug.Log("[DeathNote] PlayerInput component disabled (fallback).");
                                }
                            }
                            catch (Exception exPi)
                            {
                                Debug.Log("[DeathNote] PlayerInput 비활성화 예외: " + exPi);
                                _playerInput.enabled = false;
                            }
                            continue;
                        }

                        if (_cameraController == null &&
                            (name.Contains("CameraController") || name.Contains("MouseLook")))
                        {
                            _cameraController = mb;
                            _cameraController.enabled = false;
                            Debug.Log("[DeathNote] Camera controller disabled: " + t.FullName);
                            continue;
                        }
                    }
                }

                // 커서 잠금 해제 + 진짜 커서는 숨기고 (가짜 커서만 그림)
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;

                try
                {
                    if (CursorManager.Instance != null)
                    {
                        _cursorManager = CursorManager.Instance;
                        _cursorManager.enabled = false;
                        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        Debug.Log("[DeathNote] CursorManager.Instance disabled.");
                    }
                }
                catch (Exception exCm)
                {
                    Debug.Log("[DeathNote] CursorManager disable 예외: " + exCm);
                }

                StartCoroutine(ForceCursorFree());
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] EnterUIMode 예외: " + ex);
            }

            Debug.Log("[DeathNote] UI 모드 진입 (입력/카메라 잠금 + 가짜 커서).");
        }

        private void ExitUIMode()
        {
            if (!_uiActive)
                return;

            _uiActive = false;
            StopAllCoroutines();

            try
            {
                if (_charInput != null)
                {
                    _charInput.enabled = true;
                    Debug.Log("[DeathNote] CharacterInputControl re-enabled.");
                    _charInput = null;
                }

                if (_playerInput != null)
                {
                    Type t = _playerInput.GetType();
                    try
                    {
                        MethodInfo mActivate = t.GetMethod("ActivateInput", BINDING_FLAGS);
                        if (mActivate != null)
                        {
                            mActivate.Invoke(_playerInput, null);
                            Debug.Log("[DeathNote] PlayerInput ActivateInput() 호출 (game input restored).");
                        }
                        else
                        {
                            _playerInput.enabled = true;
                            Debug.Log("[DeathNote] PlayerInput component re-enabled (fallback).");
                        }
                    }
                    catch (Exception exPi)
                    {
                        Debug.Log("[DeathNote] PlayerInput 재활성화 예외: " + exPi);
                        _playerInput.enabled = true;
                    }

                    _playerInput = null;
                }

                if (_cameraController != null)
                {
                    _cameraController.enabled = true;
                    Debug.Log("[DeathNote] Camera controller re-enabled.");
                    _cameraController = null;
                }

                if (_cursorManager != null)
                {
                    _cursorManager.enabled = true;
                    Debug.Log("[DeathNote] CursorManager re-enabled.");
                    _cursorManager = null;
                }

                // 원래 게임 상태: 마우스 잠금 + 커서 숨김
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] ExitUIMode 예외: " + ex);
            }

            Debug.Log("[DeathNote] UI 모드 종료.");
        }

        private IEnumerator ForceCursorFree()
        {
            while (_uiActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false; // 항상 숨기고, 우리는 가짜 커서만 그림
                yield return null;
            }
        }

        // ───── 이름 헬퍼 ─────
        private static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            return name.Trim().ToLowerInvariant();
        }

        private static string SafeGetNameFromObject(object obj)
        {
            if (obj == null) return string.Empty;
            Type t = obj.GetType();

            try
            {
                FieldInfo f = t.GetField("npcName", BINDING_FLAGS);
                if (f != null)
                {
                    object v = f.GetValue(obj);
                    string s = v as string;
                    if (!string.IsNullOrEmpty(s)) return s;
                }

                f = t.GetField("displayName", BINDING_FLAGS);
                if (f != null)
                {
                    object v = f.GetValue(obj);
                    string s = v as string;
                    if (!string.IsNullOrEmpty(s)) return s;
                }

                PropertyInfo pName = t.GetProperty("Name", BINDING_FLAGS);
                if (pName != null && pName.PropertyType == typeof(string))
                {
                    object v = pName.GetValue(obj, null);
                    string s = v as string;
                    if (!string.IsNullOrEmpty(s)) return s;
                }
            }
            catch (Exception exFast)
            {
                Debug.Log("[DeathNote] SafeGetName fast 예외 (" + t.Name + "): " + exFast);
            }

            try
            {
                FieldInfo[] fields = t.GetFields(BINDING_FLAGS);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if (f == null) continue;

                    if (f.FieldType != typeof(string)) continue;

                    object v = f.GetValue(obj);
                    string s = v as string;
                    if (!string.IsNullOrEmpty(s) && s.Length <= 32)
                        return s;
                }
            }
            catch (Exception exFallback)
            {
                Debug.Log("[DeathNote] SafeGetName fallback 예외 (" + t.Name + "): " + exFallback);
            }

            return string.Empty;
        }

        // ───── OnGUI ─────
        private void OnGUI()
        {
            EnsureStyles();

            if (_uiVisible)
            {
                DrawNoteUI();
            }

            if (_pendingTarget != null && _pendingTimer > 0f)
            {
                DrawTopLeftCountdown();
                DrawDeathBubble();
            }

            // 노트가 열려 있을 때는 항상 가짜 커서를 화면 위에 그리기
            if (_uiActive)
            {
                DrawFakeCursor();
            }
        }

        private void EnsureStyles()
        {
            if (_stylesReady) return;

            if (_texBlack == null)
            {
                _texBlack = new Texture2D(1, 1);
                _texBlack.SetPixel(0, 0, Color.black);
                _texBlack.Apply();
            }

            if (_texWhite == null)
            {
                _texWhite = new Texture2D(1, 1);
                _texWhite.SetPixel(0, 0, Color.white);
                _texWhite.Apply();
            }

            _noteOuterStyle = new GUIStyle(GUI.skin.box);
            _noteOuterStyle.normal.background = _texBlack;

            _noteInnerStyle = new GUIStyle(GUI.skin.box);
            _noteInnerStyle.normal.background = _texWhite;

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 22;
            _titleStyle.normal.textColor = Color.black;

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 14;
            _labelStyle.normal.textColor = Color.black;

            _hudLabelStyle = new GUIStyle(GUI.skin.label);
            _hudLabelStyle.fontSize = 14;
            _hudLabelStyle.normal.textColor = Color.white;

            _textFieldStyle = new GUIStyle(GUI.skin.textField);
            _textFieldStyle.fontSize = 16;
            _textFieldStyle.normal.background = _texWhite;
            _textFieldStyle.focused.background = _texWhite;
            _textFieldStyle.active.background = _texWhite;
            _textFieldStyle.hover.background = _texWhite;
            _textFieldStyle.normal.textColor = Color.black;
            _textFieldStyle.focused.textColor = Color.black;
            _textFieldStyle.active.textColor = Color.black;
            _textFieldStyle.hover.textColor = Color.black;

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 16;

            _bubbleBoxStyle = new GUIStyle(GUI.skin.box);
            _bubbleBoxStyle.normal.background = _texBlack;

            _bubbleTextStyle = new GUIStyle(GUI.skin.label);
            _bubbleTextStyle.fontSize = 20;
            _bubbleTextStyle.normal.textColor = Color.white;

            // 가짜 커서용 스타일 (▲) — 색은 항상 빨간색 유지
            _cursorStyle = new GUIStyle(GUI.skin.label);
            _cursorStyle.fontSize = 20;
            _cursorStyle.normal.textColor = Color.red;

            _stylesReady = true;
        }

        private void EnsureSkullTexture()
        {
            if (_skullSearched)
                return;

            _skullSearched = true;

            try
            {
                Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
                if (sprites != null && sprites.Length > 0)
                {
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        Sprite s = sprites[i];
                        if (s == null) continue;

                        string n = s.name.ToLowerInvariant();
                        if (n.Contains("skull") || n.Contains("death") || n.Contains("dead"))
                        {
                            _skullTexture = s.texture;
                            Debug.Log("[DeathNote] 해골 텍스처 발견: " + s.name);
                            break;
                        }
                    }
                }

                if (_skullTexture == null)
                {
                    Debug.Log("[DeathNote] 해골 텍스처를 찾지 못함. ☠ 문자로 대체.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] EnsureSkullTexture 예외: " + ex);
            }
        }

        private void DrawNoteUI()
        {
            Rect outer = _windowRect;
            GUI.Box(outer, GUIContent.none, _noteOuterStyle);

            Rect inner = new Rect(
                outer.x + 8f,
                outer.y + 8f,
                outer.width - 16f,
                outer.height - 16f
            );

            GUILayout.BeginArea(inner, _noteInnerStyle);
            GUILayout.BeginVertical();

            GUILayout.Label(GetUITitleText(), _titleStyle);
            GUILayout.Space(8f);

            // ★ 네가 기억해 달라고 한 문구 - 한국어일 때는 Literal 그대로 사용
            if (GetUILang() == DeathNoteUILang.Korean)
            {
                GUILayout.Label("보스 이름을 정확히 입력하고 작성을 누르세요.", _labelStyle);
            }
            else
            {
                GUILayout.Label(GetUIDescriptionText(), _labelStyle);
            }

            // ───── 여기부터: TextField + 깜빡이는 '|' 커서 ─────
            if (_justOpened)
            {
                GUI.FocusControl("DeathNoteInput");
                _justOpened = false;
            }

            GUI.SetNextControlName("DeathNoteInput");
            bool isFocused = (GUI.GetNameOfFocusedControl() == "DeathNoteInput");

            string displayText = _inputName;

            if (isFocused)
            {
                // 0.5초마다 ON/OFF
                bool blinkOn = (Mathf.FloorToInt(Time.unscaledTime * 2f) % 2) == 0;
                if (blinkOn)
                {
                    displayText = _inputName + "|";
                }
            }

            string edited = GUILayout.TextField(displayText, _textFieldStyle);

            // 실제 입력 값에는 '|'가 들어가지 않게 제거
            if (isFocused && !string.IsNullOrEmpty(edited))
            {
                edited = edited.Replace("|", string.Empty);
            }

            _inputName = edited;
            // ───── TextField + 깜빡이 커서 끝 ─────

            Event e = Event.current;
            bool submitByEnter =
                (e.type == EventType.KeyDown &&
                 (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter));

            GUILayout.Space(8f);
            if (GUILayout.Button(GetUIButtonText(), _buttonStyle) || submitByEnter)
            {
                SubmitName();
            }

            if (_pendingTarget != null && _pendingTimer > 0f)
            {
                GUILayout.Space(10f);
                GUILayout.Label(
                    string.Format(
                        "이미 등록된 대상: {0} ({1:0}초 남음)",
                        _pendingName,
                        _pendingTimer),
                    _labelStyle);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(GetUIFooterText(), _labelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawTopLeftCountdown()
        {
            string msg = string.Format(
                "Death Note: {0} - {1:0}초 후 사망",
                _pendingName,
                _pendingTimer
            );

            Rect r = new Rect(10f, 10f, 500f, 40f);
            GUI.Label(r, msg, _hudLabelStyle);
        }

        private void DrawDeathBubble()
        {
            if (_pendingTarget == null) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Transform t = _pendingTarget.transform;
            if (t == null) return;

            Vector3 worldPos = t.position + Vector3.up * 2.5f;
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            if (screenPos.z <= 0f) return;

            float remaining = Mathf.Clamp(_pendingTimer, 0f, KILL_DELAY);
            int seconds = Mathf.CeilToInt(remaining);
            if (seconds < 0) seconds = 0;

            float width = 80f;
            float height = 70f;
            float x = screenPos.x - (width * 0.5f);
            float y = Screen.height - screenPos.y - height - 10f;

            Rect boxRect = new Rect(x, y, width, height);
            GUI.Box(boxRect, GUIContent.none, _bubbleBoxStyle);

            EnsureSkullTexture();

            float iconSize = 32f;
            Rect iconRect = new Rect(
                boxRect.x + (boxRect.width - iconSize) * 0.5f,
                boxRect.y + 4f,
                iconSize,
                iconSize
            );

            if (_skullTexture != null)
            {
                GUI.DrawTexture(iconRect, _skullTexture, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.Label(iconRect, "☠", _bubbleTextStyle);
            }

            Rect labelRect = new Rect(
                boxRect.x,
                boxRect.y + iconSize + 4f,
                boxRect.width,
                boxRect.height - iconSize - 8f
            );

            string text = seconds.ToString();
            GUI.Label(labelRect, text, _bubbleTextStyle);
        }

        // ───── 가짜 커서 그리기 ─────
        private void DrawFakeCursor()
        {
            // IMGUI 좌표계로 마우스 위치 가져오기
            Vector2 guiPos;

            if (Event.current != null)
            {
                guiPos = Event.current.mousePosition;
            }
            else
            {
                Vector3 mp = Input.mousePosition;
                guiPos = new Vector2(mp.x, Screen.height - mp.y);
            }

            float size = 20f;
            Rect r = new Rect(
                guiPos.x - size * 0.3f,
                guiPos.y - size * 0.1f,
                size,
                size
            );

            // ★ 가짜 커서 색상은 항상 빨간색
            _cursorStyle.normal.textColor = Color.red;
            GUI.Label(r, "▲", _cursorStyle);
        }

        // ───── 이름 입력 처리 ─────
        private void SubmitName()
        {
            string raw = _inputName;
            if (string.IsNullOrEmpty(raw))
            {
                Debug.Log("[DeathNote] 이름이 비어 있음");
                return;
            }

            string normInput = NormalizeName(raw);
            if (string.IsNullOrEmpty(normInput))
            {
                Debug.Log("[DeathNote] NormalizeName 결과가 비어 있음");
                return;
            }

            // 1단계: 우리가 가지고 있는 BossEntries 목록에서 매핑 시도
            string searchKeyword = ResolveBossSearchKeyword(normInput);
            if (string.IsNullOrEmpty(searchKeyword))
            {
                // 2단계: 목록에 없으면, 그냥 입력한 글자를 그대로 검색키로 사용
                searchKeyword = raw;
                Debug.Log("[DeathNote] BossEntries에 없는 이름, 직접 검색 시도: " + raw);
            }
            else
            {
                Debug.Log("[DeathNote] BossEntries 매핑 사용: 입력=" + raw + " -> 검색키=" + searchKeyword);
            }

            // 3단계: 현재 씬에서 해당 이름을 가진 보스 찾기
            Component target = FindBossByKeyword(searchKeyword);
            if (target == null)
            {
                Debug.Log("[DeathNote] 씬에서 보스를 찾지 못함. 입력=" + raw + ", 검색키=" + searchKeyword);
                return;
            }

            // 4단계: 10초 뒤 사망 예약
            _pendingTarget = target;
            _pendingName = raw;
            _pendingTimer = KILL_DELAY;

            Debug.Log("[DeathNote] " + raw + " 에게 " + KILL_DELAY + "초 후 사망 선고. (검색키=" + searchKeyword + ")");

            // 노트는 자동으로 닫기
            if (_uiVisible)
                CloseUI();
        }

        private string ResolveBossSearchKeyword(string normInput)
        {
            for (int i = 0; i < BossEntries.Length; i++)
            {
                BossEntry e = BossEntries[i];
                if (e == null || string.IsNullOrEmpty(e.Input) || string.IsNullOrEmpty(e.Search))
                    continue;

                string normEntry = NormalizeName(e.Input);

                if (normEntry == normInput ||
                    normEntry.Contains(normInput) ||
                    normInput.Contains(normEntry))
                {
                    return e.Search;
                }
            }
            return string.Empty;
        }

        // ───── 보스 찾기 ─────
        private Component FindBossByKeyword(string searchKeyword)
        {
            string normKey = NormalizeName(searchKeyword);
            if (string.IsNullOrEmpty(normKey))
                return null;

            List<string> debugNames = new List<string>();

            // 1) CharacterMainControl 쪽에서 이름 찾기
            try
            {
                CharacterMainControl[] chars =
                    UnityEngine.Object.FindObjectsOfType<CharacterMainControl>();
                if (chars != null && chars.Length > 0)
                {
                    for (int i = 0; i < chars.Length; i++)
                    {
                        CharacterMainControl c = chars[i];
                        if (c == null) continue;

                        string name = SafeGetNameFromObject(c);
                        if (string.IsNullOrEmpty(name)) continue;

                        debugNames.Add("[CMC] " + name);

                        string normName = NormalizeName(name);
                        if (normName.Contains(normKey) || normKey.Contains(normName))
                        {
                            Debug.Log("[DeathNote] FindBossByKeyword: CharacterMainControl 매치: " + name);
                            return c;
                        }
                    }
                }
            }
            catch (Exception exChars)
            {
                Debug.Log("[DeathNote] FindBossByKeyword CharacterMainControl 예외: " + exChars);
            }

            // 2) Health 쪽에서 이름 찾기
            try
            {
                Health[] hs = UnityEngine.Object.FindObjectsOfType<Health>();
                if (hs != null && hs.Length > 0)
                {
                    for (int i = 0; i < hs.Length; i++)
                    {
                        Health h = hs[i];
                        if (h == null) continue;

                        string name = SafeGetNameFromObject(h);
                        if (string.IsNullOrEmpty(name))
                        {
                            CharacterMainControl maybeChar = h.GetComponent<CharacterMainControl>();
                            if (maybeChar != null)
                                name = SafeGetNameFromObject(maybeChar);
                        }

                        if (string.IsNullOrEmpty(name)) continue;

                        debugNames.Add("[HP ] " + name);

                        string normName = NormalizeName(name);
                        if (normName.Contains(normKey) || normKey.Contains(normName))
                        {
                            Debug.Log("[DeathNote] FindBossByKeyword: Health 매치: " + name);
                            return h;
                        }
                    }
                }
            }
            catch (Exception exHealth)
            {
                Debug.Log("[DeathNote] FindBossByKeyword Health 예외: " + exHealth);
            }

            // 3) BossHealthHUD에 등록된 보스에서 찾아보기 (HUD 연동)
            Component fromHud = FindBossFromBossHealthHUD(searchKeyword, normKey);
            if (fromHud != null)
            {
                return fromHud;
            }

            // 4) 여전히 못 찾았으면 디버그 로그 출력
            if (debugNames.Count > 0)
            {
                Debug.Log("[DeathNote] FindBossByKeyword: 보스 미발견. 현재 감지된 이름 목록:");
                for (int i = 0; i < debugNames.Count; i++)
                {
                    Debug.Log("  " + debugNames[i]);
                }
            }
            else
            {
                Debug.Log("[DeathNote] FindBossByKeyword: 감지된 캐릭터/Health 없음");
            }

            return null;
        }

        // ───── BossHealthHUD 연동: HUD에 잡힌 보스에서 찾아오기 ─────
        private Component FindBossFromBossHealthHUD(string searchKeyword, string normKey)
        {
            try
            {
                MonoBehaviour[] all = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                if (all == null || all.Length == 0)
                    return null;

                MonoBehaviour manager = null;

                // bosshealthhud.BossHealthHUDManager 찾기
                for (int i = 0; i < all.Length; i++)
                {
                    MonoBehaviour mb = all[i];
                    if (mb == null) continue;

                    Type t = mb.GetType();
                    if (t.Name == "BossHealthHUDManager")
                    {
                        manager = mb;
                        break;
                    }
                }

                if (manager == null)
                    return null;

                Type mt = manager.GetType();
                FieldInfo fList = mt.GetField("_bossList", BINDING_FLAGS);
                if (fList == null)
                    return null;

                object listObj = fList.GetValue(manager);
                IList list = listObj as IList;
                if (list == null || list.Count == 0)
                    return null;

                List<string> names = new List<string>();
                Component firstComp = null;

                for (int i = 0; i < list.Count; i++)
                {
                    object elem = list[i];
                    if (elem == null) continue;

                    Component comp = elem as Component;
                    if (comp == null) continue;
                    if (firstComp == null)
                        firstComp = comp;

                    string name = SafeGetNameFromObject(elem);
                    if (!string.IsNullOrEmpty(name))
                    {
                        names.Add(name);
                        string normName = NormalizeName(name);
                        if (normName.Contains(normKey) || normKey.Contains(normName))
                        {
                            Debug.Log("[DeathNote] FindBossFromBossHealthHUD: 이름 매치: " + name);
                            return comp;
                        }
                    }
                }

                // 디버그용 후보 리스트 출력
                if (names.Count > 0)
                {
                    Debug.Log("[DeathNote] FindAnyBossFromBossHealthHUD: 후보 보스: " +
                              string.Join(", ", names.ToArray()));
                }

                // 이름은 안 맞았지만 HUD에 등록된 첫 번째 보스라도 반환 (최후 수단)
                if (firstComp != null)
                {
                    Debug.Log("[DeathNote] FindBossFromBossHealthHUD: 이름 일치는 없지만 BossHealthHUD에서 보스 선택: " +
                              SafeGetNameFromObject(firstComp));
                    return firstComp;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] FindBossFromBossHealthHUD 예외: " + ex);
            }

            return null;
        }

        // ───── 실제 처형 ─────
        private void ExecuteDeath(Component target, string name)
        {
            if (target == null)
            {
                Debug.Log("[DeathNote] 대상이 이미 사라짐: " + name);
                return;
            }

            bool killed = false;
            Health health = null;

            try
            {
                health = FindHealthForTarget(target);
                if (health != null)
                {
                    bool done = KillByHealthReflection(health);
                    if (done)
                    {
                        Debug.Log("[DeathNote] Health 기반 처치 성공: " + name);
                        killed = true;
                    }
                }

                CharacterMainControl cmc = target as CharacterMainControl;
                if (cmc != null && !killed)
                {
                    bool done2 = KillByCharacterReflection(cmc);
                    if (done2)
                    {
                        Debug.Log("[DeathNote] CharacterMainControl 기반 처치 성공: " + name);
                        killed = true;
                    }
                }

                if (killed)
                {
                    if (health != null)
                    {
                        AddDeadLockHealth(health);
                    }

                    // 전리품/사망 처리 관련 메서드들을 한 번 더 호출 시도
                    CallDeathMethodsOnAttachedComponents(target);

                    // 시체가 계속 움직이는 것처럼 보이지 않도록 동결
                    FreezeCorpseComponents(target);
                }
                else
                {
                    Debug.Log("[DeathNote] 처치 실패 - Kill/Die/HP 필드 찾지 못함: " + name);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] ExecuteDeath 예외: " + ex);
            }
        }

        private Health FindHealthForTarget(Component target)
        {
            if (target == null) return null;

            Health h = target.GetComponent<Health>();
            if (h == null) h = target.GetComponentInChildren<Health>();
            if (h == null) h = target.GetComponentInParent<Health>();
            return h;
        }

        private CharacterMainControl FindMainCharacter()
        {
            try
            {
                CharacterMainControl[] all =
                    UnityEngine.Object.FindObjectsOfType<CharacterMainControl>();
                if (all == null || all.Length == 0)
                    return null;

                for (int i = 0; i < all.Length; i++)
                {
                    CharacterMainControl c = all[i];
                    if (c == null) continue;

                    try
                    {
                        if (c.IsMainCharacter)
                            return c;
                    }
                    catch
                    {
                    }
                }

                return all[0];
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] FindMainCharacter 예외: " + ex);
            }

            return null;
        }

        // ─────────────────────────────────────────────
        // DuckovMenu와 같은 방식으로 Health.Hurt(...)를 호출해서
        // "정상적인 데미지/사망 처리"를 타게 만드는 함수 (전리품 드랍용)
        // ─────────────────────────────────────────────
        private bool TryKillWithHurt(Health health)
        {
            if (health == null) return false;

            try
            {
                Type type = health.GetType();

                // CurrentHealth 프로퍼티(있으면 나중에 1로 세팅해서 확실히 죽이기)
                PropertyInfo propCurrentHealth = type.GetProperty("CurrentHealth", BINDING_FLAGS);
                if (propCurrentHealth == null)
                {
                    propCurrentHealth = type.GetProperty("CurrentHealth");
                }

                bool success = false;

                // 우선 Hurt 메서드를 찾는다
                MethodInfo hurt = type.GetMethod("Hurt", BINDING_FLAGS);
                if (hurt == null)
                {
                    hurt = type.GetMethod("Hurt");
                }

                if (hurt != null)
                {
                    ParameterInfo[] ps = hurt.GetParameters();

                    // 1) Hurt(DamageInfo) 타입인 경우
                    if (ps.Length == 1 && ps[0].ParameterType.Name == "DamageInfo")
                    {
                        Type damageType = ps[0].ParameterType;
                        object dmgInfo = Activator.CreateInstance(damageType);

                        // 데미지 값 크게
                        FieldInfo fDamageValue = damageType.GetField("damageValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fDamageValue != null)
                        {
                            if (fDamageValue.FieldType == typeof(float)) fDamageValue.SetValue(dmgInfo, 99999f);
                            else if (fDamageValue.FieldType == typeof(int)) fDamageValue.SetValue(dmgInfo, 99999);
                        }

                        FieldInfo fFinalDamage = damageType.GetField("finalDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fFinalDamage != null)
                        {
                            if (fFinalDamage.FieldType == typeof(float)) fFinalDamage.SetValue(dmgInfo, 99999f);
                            else if (fFinalDamage.FieldType == typeof(int)) fFinalDamage.SetValue(dmgInfo, 99999);
                        }

                        // fromCharacter = 플레이어
                        FieldInfo fFromChar = damageType.GetField("fromCharacter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fFromChar != null)
                        {
                            CharacterMainControl mainChar = FindMainCharacter();
                            if (mainChar != null && fFromChar.FieldType.IsAssignableFrom(mainChar.GetType()))
                            {
                                fFromChar.SetValue(dmgInfo, mainChar);
                            }
                        }

                        // ignoreArmor / ignoreDifficulty
                        FieldInfo fIgnoreArmor = damageType.GetField("ignoreArmor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fIgnoreArmor != null && fIgnoreArmor.FieldType == typeof(bool))
                        {
                            fIgnoreArmor.SetValue(dmgInfo, true);
                        }

                        FieldInfo fIgnoreDiff = damageType.GetField("ignoreDifficulty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fIgnoreDiff != null && fIgnoreDiff.FieldType == typeof(bool))
                        {
                            fIgnoreDiff.SetValue(dmgInfo, true);
                        }

                        // damagePoint = 보스 위치
                        FieldInfo fDamagePoint = damageType.GetField("damagePoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fDamagePoint != null && fDamagePoint.FieldType == typeof(Vector3))
                        {
                            Vector3 point = Vector3.zero;
                            try
                            {
                                point = health.transform.position;
                            }
                            catch
                            {
                            }
                            fDamagePoint.SetValue(dmgInfo, point);
                        }

                        // elementFactors 같은 추가 필드 있으면 기본 인스턴스 만들어 넣기
                        FieldInfo fElementFactors = damageType.GetField("elementFactors", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fElementFactors != null)
                        {
                            try
                            {
                                Type efType = fElementFactors.FieldType;
                                if (efType != null && efType.GetConstructor(Type.EmptyTypes) != null)
                                {
                                    object efInstance = Activator.CreateInstance(efType);
                                    fElementFactors.SetValue(dmgInfo, efInstance);
                                }
                            }
                            catch
                            {
                            }
                        }

                        hurt.Invoke(health, new object[] { dmgInfo });
                        Debug.Log("[DeathNote] TryKillWithHurt: Hurt(DamageInfo) 호출 성공");
                        success = true;
                    }
                    // 2) Hurt( float / int ) 한 개짜리
                    else if (ps.Length == 1)
                    {
                        object dmg = null;
                        if (ps[0].ParameterType == typeof(float))
                            dmg = 99999f;
                        else if (ps[0].ParameterType == typeof(int))
                            dmg = 99999;

                        if (dmg != null)
                        {
                            hurt.Invoke(health, new object[] { dmg });
                            Debug.Log("[DeathNote] TryKillWithHurt: Hurt(bigDamage) 호출 성공");
                            success = true;
                        }
                    }
                    // 3) Hurt(dmg, something)
                    else if (ps.Length == 2)
                    {
                        object[] args = new object[2];
                        if (ps[0].ParameterType == typeof(float))
                            args[0] = 99999f;
                        else if (ps[0].ParameterType == typeof(int))
                            args[0] = 99999;
                        else
                            args[0] = null;

                        args[1] = null;
                        hurt.Invoke(health, args);
                        Debug.Log("[DeathNote] TryKillWithHurt: Hurt(dmg, null) 호출 시도");
                        success = true;
                    }
                    // 4) Hurt(dmg, ..., Vector3.zero, ...)
                    else if (ps.Length >= 3)
                    {
                        object[] args = new object[ps.Length];
                        if (ps[0].ParameterType == typeof(float))
                            args[0] = 99999f;
                        else if (ps[0].ParameterType == typeof(int))
                            args[0] = 99999;
                        else
                            args[0] = null;

                        args[1] = null;
                        args[2] = Vector3.zero;
                        for (int i = 3; i < ps.Length; i++)
                        {
                            args[i] = Type.Missing;
                        }
                        hurt.Invoke(health, args);
                        Debug.Log("[DeathNote] TryKillWithHurt: Hurt(dmg, ...) 호출 시도");
                        success = true;
                    }
                }

                // 위에서 실패했고, CurrentHealth를 쓸 수 있다면
                // CurrentHealth를 1로 만들어 놓고 "작은 데미지"로 한 번 더 Hurt 호출
                if (!success && propCurrentHealth != null && propCurrentHealth.CanWrite)
                {
                    try
                    {
                        Type chType = propCurrentHealth.PropertyType;
                        if (chType == typeof(float))
                            propCurrentHealth.SetValue(health, 1f, null);
                        else if (chType == typeof(int))
                            propCurrentHealth.SetValue(health, 1, null);

                        MethodInfo hurt2 = type.GetMethod("Hurt", BINDING_FLAGS);
                        if (hurt2 == null)
                            hurt2 = type.GetMethod("Hurt");

                        if (hurt2 != null)
                        {
                            ParameterInfo[] ps2 = hurt2.GetParameters();
                            if (ps2.Length == 1 && ps2[0].ParameterType.Name == "DamageInfo")
                            {
                                Type damageType2 = ps2[0].ParameterType;
                                object dmg2 = Activator.CreateInstance(damageType2);

                                FieldInfo fdv = damageType2.GetField("damageValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                if (fdv != null)
                                {
                                    if (fdv.FieldType == typeof(float))
                                        fdv.SetValue(dmg2, 1f);
                                    else if (fdv.FieldType == typeof(int))
                                        fdv.SetValue(dmg2, 1);
                                }

                                FieldInfo ffd = damageType2.GetField("finalDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                if (ffd != null)
                                {
                                    if (ffd.FieldType == typeof(float))
                                        ffd.SetValue(dmg2, 1f);
                                    else if (ffd.FieldType == typeof(int))
                                        ffd.SetValue(dmg2, 1);
                                }

                                FieldInfo fdp = damageType2.GetField("damagePoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                if (fdp != null && fdp.FieldType == typeof(Vector3))
                                {
                                    Vector3 point = Vector3.zero;
                                    try { point = health.transform.position; } catch { }
                                    fdp.SetValue(dmg2, point);
                                }

                                hurt2.Invoke(health, new object[] { dmg2 });
                            }
                            else if (ps2.Length >= 1)
                            {
                                object[] args = new object[ps2.Length];
                                if (ps2[0].ParameterType == typeof(float))
                                    args[0] = 1f;
                                else if (ps2[0].ParameterType == typeof(int))
                                    args[0] = 1;
                                else
                                    args[0] = null;

                                for (int i = 1; i < ps2.Length; i++)
                                {
                                    args[i] = Type.Missing;
                                }

                                hurt2.Invoke(health, args);
                            }

                            Debug.Log("[DeathNote] TryKillWithHurt: 최종 Hurt 호출 성공 (1 damage)");
                            success = true;
                        }
                    }
                    catch (Exception ex2)
                    {
                        Debug.Log("[DeathNote] TryKillWithHurt 최종 시도 예외: " + ex2);
                    }
                }

                if (!success)
                {
                    Debug.Log("[DeathNote] TryKillWithHurt: Hurt 기반 킬 실패 (Hurt 메서드 없음?)");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] TryKillWithHurt 예외: " + ex);
                return false;
            }
        }

        // ───── Health 강제 킬 ─────
        private bool KillByHealthReflection(Health health)
        {
            if (health == null) return false;

            // ★ 0단계: Hurt 기반 정상 데미지 사망 시도 (전리품/OnDead 루틴 타게)
            if (TryKillWithHurt(health))
            {
                return true;
            }

            Type t = health.GetType();
            bool changed = false;

            // 1) Kill/Die 계열 메서드 (파라미터 없음)
            try
            {
                MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m == null) continue;
                    if (m.IsSpecialName) continue;
                    if (m.GetParameters().Length != 0) continue;

                    string ln = m.Name.ToLowerInvariant();
                    if (ln.Contains("kill") || ln.Contains("die") ||
                        ln.Contains("death") || ln.Contains("dead"))
                    {
                        Debug.Log("[DeathNote] KillByHealthReflection: 메서드 호출 -> " +
                                  t.Name + "." + m.Name + "()");
                        m.Invoke(health, null);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] KillByHealthReflection direct method 예외: " + ex);
            }

            // 2) 데미지 계열 메서드 (파라미터 하나, int/float)
            try
            {
                MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m == null) continue;
                    if (m.IsSpecialName) continue;

                    ParameterInfo[] ps = m.GetParameters();
                    if (ps == null || ps.Length != 1)
                        continue;

                    string ln = m.Name.ToLowerInvariant();
                    if (!(ln.Contains("damage") || ln.Contains("hit")))
                        continue;

                    Type ptParam = ps[0].ParameterType;
                    object big;
                    if (ptParam == typeof(int))
                        big = 999999;
                    else if (ptParam == typeof(float))
                        big = 999999f;
                    else
                        continue;

                    Debug.Log("[DeathNote] KillByHealthReflection: 데미지 메서드 호출 -> " +
                              t.Name + "." + m.Name + "(" + big + ")");
                    m.Invoke(health, new object[] { big });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] KillByHealthReflection damage method 예외: " + ex);
            }

            // 3) 프로퍼티/필드 직접 조작
            try
            {
                PropertyInfo[] props = t.GetProperties(BINDING_FLAGS);
                for (int i = 0; i < props.Length; i++)
                {
                    PropertyInfo p = props[i];
                    if (p == null || !p.CanWrite) continue;

                    Type pt = p.PropertyType;
                    string n = p.Name.ToLowerInvariant();

                    if (pt == typeof(bool))
                    {
                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            p.SetValue(health, true, null);
                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 프로퍼티 true -> " +
                                      t.Name + "." + p.Name);
                        }
                        else if (n.Contains("alive"))
                        {
                            p.SetValue(health, false, null);
                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 프로퍼티 false -> " +
                                      t.Name + "." + p.Name);
                        }
                    }
                    else if (pt == typeof(int) || pt == typeof(float))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            if (pt == typeof(int))
                                p.SetValue(health, -1, null);
                            else
                                p.SetValue(health, -1f, null);

                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: hp/health 프로퍼티 -1 -> " +
                                      t.Name + "." + p.Name);
                        }
                    }
                }

                FieldInfo[] fields = t.GetFields(BINDING_FLAGS);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if (f == null) continue;

                    Type ft = f.FieldType;
                    string n = f.Name.ToLowerInvariant();

                    if (ft == typeof(bool))
                    {
                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            f.SetValue(health, true);
                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 필드 true -> " +
                                      t.Name + "." + f.Name);
                        }
                        else if (n.Contains("alive"))
                        {
                            f.SetValue(health, false);
                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 필드 false -> " +
                                      t.Name + "." + f.Name);
                        }
                    }
                }

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if (f == null) continue;

                    Type ft = f.FieldType;
                    string n = f.Name.ToLowerInvariant();

                    if (ft == typeof(int) || ft == typeof(float))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            if (ft == typeof(int))
                                f.SetValue(health, -1);
                            else
                                f.SetValue(health, -1f);

                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: hp/health 필드 -1 -> " +
                                      t.Name + "." + f.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] KillByHealthReflection 필드/프로퍼티 조작 예외: " + ex);
            }

            return changed;
        }

        // ───── CharacterMainControl 강제 킬 (보조용) ─────
        private bool KillByCharacterReflection(CharacterMainControl character)
        {
            if (character == null) return false;

            Type t = character.GetType();
            bool changed = false;

            try
            {
                MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m == null) continue;
                    if (m.IsSpecialName) continue;
                    if (m.GetParameters().Length != 0) continue;

                    string ln = m.Name.ToLowerInvariant();
                    if (ln.Contains("kill") || ln.Contains("die") ||
                        ln.Contains("death") || ln.Contains("dead"))
                    {
                        Debug.Log("[DeathNote] KillByCharacterReflection: 메서드 호출 -> " +
                                  t.Name + "." + m.Name + "()");
                        m.Invoke(character, null);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] KillByCharacterReflection direct method 예외: " + ex);
            }

            try
            {
                PropertyInfo[] props = t.GetProperties(BINDING_FLAGS);
                for (int i = 0; i < props.Length; i++)
                {
                    PropertyInfo p = props[i];
                    if (p == null || !p.CanWrite) continue;

                    Type pt = p.PropertyType;
                    string n = p.Name.ToLowerInvariant();

                    if (pt == typeof(bool))
                    {
                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            p.SetValue(character, true, null);
                            changed = true;
                        }
                    }
                }

                FieldInfo[] fields = t.GetFields(BINDING_FLAGS);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if (f == null) continue;

                    Type ft = f.FieldType;
                    string n = f.Name.ToLowerInvariant();

                    if (ft == typeof(bool))
                    {
                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            f.SetValue(character, true);
                            changed = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] KillByCharacterReflection 필드/프로퍼티 조작 예외: " + ex);
            }

            return changed;
        }

        // ───── 시체 움직임 동결 ─────
        private void FreezeCorpseComponents(Component target)
        {
            if (target == null) return;
            GameObject go = target.gameObject;
            if (go == null) return;

            try
            {
                Animator[] anims = go.GetComponentsInChildren<Animator>(true);
                if (anims != null)
                {
                    for (int i = 0; i < anims.Length; i++)
                    {
                        Animator anim = anims[i];
                        if (anim == null) continue;

                        anim.speed = 0f;
                        anim.enabled = false;
                        Debug.Log("[DeathNote] FreezeCorpse: Animator disabled - " + anim.gameObject.name);
                    }
                }

                Rigidbody[] rbs = go.GetComponentsInChildren<Rigidbody>(true);
                if (rbs != null)
                {
                    for (int i = 0; i < rbs.Length; i++)
                    {
                        Rigidbody rb = rbs[i];
                        if (rb == null) continue;

                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        Debug.Log("[DeathNote] FreezeCorpse: Rigidbody velocity cleared - " + rb.gameObject.name);
                    }
                }

                CharacterController[] ccs = go.GetComponentsInChildren<CharacterController>(true);
                if (ccs != null)
                {
                    for (int i = 0; i < ccs.Length; i++)
                    {
                        CharacterController cc = ccs[i];
                        if (cc == null) continue;

                        cc.enabled = false;
                        Debug.Log("[DeathNote] FreezeCorpse: CharacterController disabled - " + cc.gameObject.name);
                    }
                }
            }
            catch (Exception exOuter)
            {
                Debug.Log("[DeathNote] FreezeCorpseComponents 예외: " + exOuter);
            }
        }

        // ───── 전리품/사망 처리 메서드 반사 호출 ─────
        private void CallDeathMethodsOnAttachedComponents(Component target)
        {
            if (target == null) return;
            GameObject go = target.gameObject;
            if (go == null) return;

            bool anyCalled = false;

            try
            {
                MonoBehaviour[] comps = go.GetComponentsInChildren<MonoBehaviour>(true);
                if (comps != null && comps.Length > 0)
                {
                    for (int i = 0; i < comps.Length; i++)
                    {
                        MonoBehaviour comp = comps[i];
                        if (comp == null) continue;

                        Type t = comp.GetType();
                        MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                        for (int j = 0; j < methods.Length; j++)
                        {
                            MethodInfo m = methods[j];
                            if (m == null) continue;
                            if (m.IsSpecialName) continue;
                            if (m.GetParameters().Length != 0) continue;

                            string ln = m.Name.ToLowerInvariant();
                            if (ln.Contains("ondead") ||
                                ln.Contains("ondeath") ||
                                ln.Contains("dead") ||
                                ln.Contains("death") ||
                                ln.Contains("droploot") ||
                                ln.Contains("loot"))
                            {
                                try
                                {
                                    m.Invoke(comp, null);
                                    anyCalled = true;
                                    Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents: " +
                                              t.Name + "." + m.Name + "()");
                                }
                                catch (Exception exInner)
                                {
                                    Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents inner 예외: " + exInner);
                                }
                            }
                        }
                    }
                }

                Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents 호출됨, anyCalled=" + anyCalled);
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents 예외: " + ex);
            }
        }

        // ───── 죽은 보스 Health를 계속 죽은 상태로 유지 ─────
        private void AddDeadLockHealth(Health health)
        {
            if (health == null) return;
            if (!_deadLockList.Contains(health))
            {
                _deadLockList.Add(health);
            }
        }

        private void EnsureHealthStaysDead(Health health)
        {
            if (health == null) return;

            try
            {
                Type t = health.GetType();

                PropertyInfo[] props = t.GetProperties(BINDING_FLAGS);
                for (int i = 0; i < props.Length; i++)
                {
                    PropertyInfo p = props[i];
                    if (p == null || !p.CanWrite) continue;

                    Type pt = p.PropertyType;
                    string n = p.Name.ToLowerInvariant();

                    if (pt == typeof(bool))
                    {
                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            p.SetValue(health, true, null);
                        }
                        else if (n.Contains("alive"))
                        {
                            p.SetValue(health, false, null);
                        }
                    }
                    else if (pt == typeof(int) || pt == typeof(float))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            if (pt == typeof(int))
                                p.SetValue(health, -1, null);
                            else
                                p.SetValue(health, -1f, null);
                        }
                    }
                }

                FieldInfo[] fields = t.GetFields(BINDING_FLAGS);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if (f == null) continue;

                    Type ft = f.FieldType;
                    string n = f.Name.ToLowerInvariant();

                    if (ft == typeof(bool))
                    {
                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            f.SetValue(health, true);
                        }
                        else if (n.Contains("alive"))
                        {
                            f.SetValue(health, false);
                        }
                    }
                    else if (ft == typeof(int) || ft == typeof(float))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            if (ft == typeof(int))
                                f.SetValue(health, -1);
                            else
                                f.SetValue(health, -1f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] EnsureHealthStaysDead 예외: " + ex);
            }
        }
    }
}
