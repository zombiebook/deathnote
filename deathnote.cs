using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ItemStatsSystem;
using UnityEngine;
using Duckov;   // CharacterMainControl, Health

namespace deathnote
{
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

        // ───── 언어 설정 ─────
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
                    return "ボスの名前を正確に入力して「作成」を押してください。";
                case DeathNoteUILang.English:
                    return "Enter the boss name exactly and press Write.";
                default:
                    // ★ 이 문장은 네가 기억해 달라고 한 그대로 유지
                    return "보스 이름을 정확히 입력하고 작성을 누르세요.";
            }
        }

        private string GetUIFooterText()
        {
            switch (GetUILang())
            {
                case DeathNoteUILang.Japanese:
                    return "Insert: ノートを開く/閉じる   /   Esc: 閉じる";
                case DeathNoteUILang.English:
                    return "Insert: Open/Close the note   /   Esc: Close";
                default:
                    return "Insert: 노트 열기/닫기   /   Esc: 닫기";
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
            new BossEntry { Input = "???",            Search = "???" },
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

        // ───── UI 상태 ─────
        private bool _uiVisible;
        private bool _uiActive;
        private Rect _windowRect;
        private string _inputName = string.Empty;

        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _hudLabelStyle;
        private GUIStyle _textFieldStyle;
        private GUIStyle _buttonStyle;

        private GUIStyle _bubbleBoxStyle;
        private GUIStyle _bubbleTextStyle;

        private GUIStyle _noteOuterStyle;
        private GUIStyle _noteInnerStyle;
        private Texture2D _texBlack;
        private Texture2D _texWhite;

        private bool _stylesReady;
        private bool _justOpened;

        // 말풍선 해골
        private Texture2D _skullTexture;
        private bool _skullSearched;

        // 메뉴 포커스용
        private MonoBehaviour _charInput;
        private MonoBehaviour _playerInput;
        private MonoBehaviour _cameraController;
        private MonoBehaviour _cursorManager;

        // 예약된 죽음
        private Component _pendingTarget;
        private float _pendingTimer;
        private string _pendingName;

        // 이미 죽인 보스 Health를 계속 "죽은 채"로 유지
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
            // UI가 활성화된 동안에는 커서를 계속 보이게 유지
            if (_uiActive)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // 이미 죽인 보스 HP가 다시 차지 않도록 매 프레임 잠금
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

        // ───── UI 토글/포커스 ─────
        private void ToggleUI()
        {
            if (_uiVisible)
            {
                CloseUI();
            }
            else
            {
                OpenUI();
            }
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

            // 노트 열릴 때 커서 보이게 + 잠금 해제
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

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
                            _playerInput.enabled = false;
                            Debug.Log("[DeathNote] PlayerInput component disabled.");
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

                        if (_cursorManager == null && name.Contains("CursorManager"))
                        {
                            _cursorManager = mb;
                            _cursorManager.enabled = false;
                            Debug.Log("[DeathNote] CursorManager disabled: " + t.FullName);
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] EnterUIMode 예외: " + ex);
            }

            Debug.Log("[DeathNote] UI 모드 진입 (입력/카메라 잠금 해제).");
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
                    _playerInput.enabled = true;
                    Debug.Log("[DeathNote] PlayerInput component re-enabled.");
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
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] ExitUIMode 예외: " + ex);
            }

            // 여기서는 커서는 건드리지 않고, 게임 원래 로직에 맡김
            Debug.Log("[DeathNote] UI 모드 종료.");
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
                    string s2 = v as string;
                    if (!string.IsNullOrEmpty(s2)) return s2;
                }

                PropertyInfo pName = t.GetProperty("Name", BINDING_FLAGS);
                if (pName != null && pName.PropertyType == typeof(string))
                {
                    object v = pName.GetValue(obj, null);
                    string s3 = v as string;
                    if (!string.IsNullOrEmpty(s3)) return s3;
                }
            }
            catch (Exception exFast)
            {
                Debug.Log("[DeathNote] SafeGetName fast 예외 (" + t.Name + "): " + exFast);
            }

            try
            {
                FieldInfo[] allFields = t.GetFields(BINDING_FLAGS);
                for (int i = 0; i < allFields.Length; i++)
                {
                    FieldInfo f = allFields[i];
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

            if (!_uiVisible)
            {
                if (_pendingTarget != null && _pendingTimer > 0f)
                {
                    DrawTopLeftCountdown();
                }
            }
            else
            {
                DrawNoteUI();
            }

            if (_pendingTarget != null && _pendingTimer > 0f)
            {
                DrawDeathBubble();
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
            GUILayout.Label(GetUIDescriptionText(), _labelStyle);

            if (_justOpened)
            {
                GUI.FocusControl("DeathNoteInput");
                _justOpened = false;
            }

            GUI.SetNextControlName("DeathNoteInput");
            _inputName = GUILayout.TextField(_inputName, _textFieldStyle);

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

            string searchKeyword = ResolveBossSearchKeyword(normInput);
            if (string.IsNullOrEmpty(searchKeyword))
            {
                Debug.Log("[DeathNote] BossEntries에 없는 이름: " + raw);
                return;
            }

            Component target = FindBossByKeyword(searchKeyword);
            if (target == null)
            {
                Debug.Log("[DeathNote] 씬에서 보스를 찾지 못함. 입력=" + raw + ", 검색키=" + searchKeyword);
                return;
            }

            _pendingTarget = target;
            _pendingName = raw;
            _pendingTimer = KILL_DELAY;

            Debug.Log("[DeathNote] " + raw + " 에게 " + KILL_DELAY + "초 후 사망 선고. (검색키=" + searchKeyword + ")");

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

            // 1) BossHealthHUD에서 이름 매치
            Component fromHud = FindBossFromBossHealthHUD(normKey);
            if (fromHud != null)
                return fromHud;

            // 2) BossHealthHUD에 등록된 아무 보스
            Component anyFromHud = FindAnyBossFromBossHealthHUD();
            if (anyFromHud != null)
            {
                string n = SafeGetNameFromObject(anyFromHud);
                Debug.Log("[DeathNote] FindBossByKeyword: 이름 일치는 없지만 BossHealthHUD에서 보스 선택: " + n);
                return anyFromHud;
            }

            // 3) 씬 전체 CharacterMainControl / Health 스캔
            List<string> debugNames = new List<string>();

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

        private Component FindBossFromBossHealthHUD(string normKey)
        {
            try
            {
                MonoBehaviour[] all = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                if (all == null || all.Length == 0)
                    return null;

                for (int i = 0; i < all.Length; i++)
                {
                    MonoBehaviour mb = all[i];
                    if (mb == null) continue;

                    Type t = mb.GetType();
                    string typeNameLower = t.Name.ToLowerInvariant();
                    if (!typeNameLower.Contains("bosshealthhud"))
                        continue;

                    FieldInfo[] fields = t.GetFields(BINDING_FLAGS);
                    for (int f = 0; f < fields.Length; f++)
                    {
                        FieldInfo fi = fields[f];
                        if (fi == null) continue;

                        Type ft = fi.FieldType;
                        if (!typeof(IEnumerable).IsAssignableFrom(ft))
                            continue;

                        if (!ft.IsGenericType)
                            continue;

                        Type[] args = ft.GetGenericArguments();
                        if (args == null || args.Length != 1 || args[0] != typeof(CharacterMainControl))
                            continue;

                        object listObj = fi.GetValue(mb);
                        if (listObj == null) continue;

                        IEnumerable enumerable = listObj as IEnumerable;
                        if (enumerable == null) continue;

                        foreach (object elem in enumerable)
                        {
                            CharacterMainControl c = elem as CharacterMainControl;
                            if (c == null) continue;

                            string name = SafeGetNameFromObject(c);
                            if (string.IsNullOrEmpty(name)) continue;

                            string normName = NormalizeName(name);
                            if (normName.Contains(normKey) || normKey.Contains(normName))
                            {
                                Debug.Log("[DeathNote] FindBossFromBossHealthHUD: " + t.Name + "." + fi.Name +
                                          " 리스트에서 매치: " + name);
                                return c;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] FindBossFromBossHealthHUD 예외: " + ex);
            }

            return null;
        }

        private Component FindAnyBossFromBossHealthHUD()
        {
            try
            {
                MonoBehaviour[] all = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                if (all == null || all.Length == 0)
                    return null;

                for (int i = 0; i < all.Length; i++)
                {
                    MonoBehaviour mb = all[i];
                    if (mb == null) continue;

                    Type t = mb.GetType();
                    string typeNameLower = t.Name.ToLowerInvariant();
                    if (!typeNameLower.Contains("bosshealthhud"))
                        continue;

                    FieldInfo[] fields = t.GetFields(BINDING_FLAGS);
                    for (int f = 0; f < fields.Length; f++)
                    {
                        FieldInfo fi = fields[f];
                        if (fi == null) continue;

                        Type ft = fi.FieldType;
                        if (!typeof(IEnumerable).IsAssignableFrom(ft))
                            continue;

                        if (!ft.IsGenericType)
                            continue;

                        Type[] args = ft.GetGenericArguments();
                        if (args == null || args.Length != 1 || args[0] != typeof(CharacterMainControl))
                            continue;

                        object listObj = fi.GetValue(mb);
                        if (listObj == null) continue;

                        IEnumerable enumerable = listObj as IEnumerable;
                        if (enumerable == null) continue;

                        foreach (object elem in enumerable)
                        {
                            CharacterMainControl c = elem as CharacterMainControl;
                            if (c == null) continue;

                            string name = SafeGetNameFromObject(c);
                            Debug.Log("[DeathNote] FindAnyBossFromBossHealthHUD: 후보 보스: " + name +
                                      " (" + t.Name + "." + fi.Name + ")");
                            return c;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] FindAnyBossFromBossHealthHUD 예외: " + ex);
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
                // 1) DamageReceiver 시도
                if (KillViaDamageReceiver(target))
                {
                    Debug.Log("[DeathNote] DamageReceiver 기반 처치 시도 완료: " + name);
                    killed = true;
                }

                // 2) Health 리플렉션
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

                // 3) CharacterMainControl 리플렉션
                CharacterMainControl cmc = target as CharacterMainControl;
                if (cmc != null)
                {
                    bool done2 = KillByCharacterReflection(cmc);
                    if (done2)
                    {
                        Debug.Log("[DeathNote] CharacterMainControl 기반 처치 성공: " + name);
                        killed = true;
                    }
                }

                // 4) 죽음 이벤트/메서드 강제 호출 + 시체 동결 + 데드락 등록
                if (killed)
                {
                    if (health != null)
                    {
                        FireHealthOnDeadEvent(health);
                    }

                    bool called = CallDeathMethodsOnAttachedComponents(target, health);
                    Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents 호출됨, anyCalled=" + called);

                    FreezeCorpseComponents(target);

                    if (health != null)
                    {
                        AddDeadLockHealth(health);
                    }
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

        // ───── DamageReceiver 경유 처치 ─────
        private bool KillViaDamageReceiver(Component target)
        {
            if (target == null) return false;

            Health targetHealth = FindHealthForTarget(target);
            GameObject go = target.gameObject;
            if (go == null) return false;

            Component[] compsChildren = go.GetComponentsInChildren<Component>(true);
            Component[] compsParents = go.GetComponentsInParent<Component>(true);

            bool anySuccess = false;

            anySuccess |= TryKillViaDamageReceiverList(compsChildren, targetHealth);
            if (anySuccess) return true;

            anySuccess |= TryKillViaDamageReceiverList(compsParents, targetHealth);
            return anySuccess;
        }

        private bool TryKillViaDamageReceiverList(Component[] comps, Health targetHealth)
        {
            if (comps == null || comps.Length == 0) return false;

            for (int i = 0; i < comps.Length; i++)
            {
                Component c = comps[i];
                if (c == null) continue;

                Type ct = c.GetType();
                string tn = ct.Name.ToLowerInvariant();
                if (!tn.Contains("damagereceiver"))
                    continue;

                if (TryInvokeDamageOnReceiver(c, targetHealth))
                    return true;
            }

            return false;
        }

        private bool TryInvokeDamageOnReceiver(Component receiver, Health targetHealth)
        {
            if (receiver == null) return false;

            Type rt = receiver.GetType();
            bool sameHealth = false;

            try
            {
                FieldInfo[] rfields = rt.GetFields(BINDING_FLAGS);
                for (int i = 0; i < rfields.Length; i++)
                {
                    FieldInfo f = rfields[i];
                    if (f == null) continue;

                    if (f.FieldType == typeof(Health))
                    {
                        object v = f.GetValue(receiver);
                        if (v == (object)targetHealth && v != null)
                        {
                            sameHealth = true;
                            break;
                        }
                    }
                }

                if (!sameHealth && targetHealth != null)
                {
                    PropertyInfo[] rprops = rt.GetProperties(BINDING_FLAGS);
                    for (int i = 0; i < rprops.Length; i++)
                    {
                        PropertyInfo p = rprops[i];
                        if (p == null || !p.CanRead) continue;

                        if (p.PropertyType == typeof(Health))
                        {
                            object v = p.GetValue(receiver, null);
                            if (v == (object)targetHealth && v != null)
                            {
                                sameHealth = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] TryInvokeDamageOnReceiver Health 매칭 예외: " + ex);
            }

            CharacterMainControl mainChar = FindMainCharacter();

            try
            {
                MethodInfo chosenMethod = null;
                Type damageInfoType = null;

                MethodInfo[] methods = rt.GetMethods(BINDING_FLAGS);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m == null) continue;
                    if (m.IsSpecialName) continue;

                    ParameterInfo[] ps = m.GetParameters();
                    if (ps == null || ps.Length < 1 || ps.Length > 2)
                        continue;

                    string mn = m.Name.ToLowerInvariant();
                    bool looksLikeDamage =
                        mn.Contains("damage") || mn.Contains("hit") || mn.Contains("attack");

                    if (!looksLikeDamage)
                        continue;

                    for (int pIndex = 0; pIndex < ps.Length; pIndex++)
                    {
                        Type pt = ps[pIndex].ParameterType;
                        string ptName = pt.Name.ToLowerInvariant();
                        if (ptName.Contains("damageinfo"))
                        {
                            chosenMethod = m;
                            damageInfoType = pt;
                            break;
                        }
                    }

                    if (chosenMethod != null)
                        break;
                }

                if (chosenMethod == null || damageInfoType == null)
                {
                    Debug.Log("[DeathNote] TryInvokeDamageOnReceiver: DamageInfo 유사 메서드를 찾지 못함 (" + rt.Name + ")");
                    return false;
                }

                object dmg = Activator.CreateInstance(damageInfoType);

                try
                {
                    FieldInfo[] dfs = damageInfoType.GetFields(BINDING_FLAGS);
                    for (int i = 0; i < dfs.Length; i++)
                    {
                        FieldInfo f = dfs[i];
                        if (f == null) continue;

                        Type ft = f.FieldType;
                        string fn = f.Name.ToLowerInvariant();

                        if (typeof(CharacterMainControl).IsAssignableFrom(ft) || fn.Contains("fromcharacter"))
                        {
                            if (mainChar != null)
                            {
                                f.SetValue(dmg, mainChar);
                            }
                            continue;
                        }

                        if (typeof(Component).IsAssignableFrom(ft) && fn.Contains("receiver"))
                        {
                            f.SetValue(dmg, receiver);
                            continue;
                        }

                        if (ft == typeof(Health) && targetHealth != null && fn.Contains("health"))
                        {
                            f.SetValue(dmg, targetHealth);
                            continue;
                        }

                        if ((ft == typeof(int) || ft == typeof(float)) &&
                            (fn.Contains("damage") || fn.Contains("dmg") || fn.Contains("amount")))
                        {
                            if (ft == typeof(int))
                                f.SetValue(dmg, 999999);
                            else
                                f.SetValue(dmg, 999999f);
                            continue;
                        }
                    }

                    PropertyInfo[] dps = damageInfoType.GetProperties(BINDING_FLAGS);
                    for (int i = 0; i < dps.Length; i++)
                    {
                        PropertyInfo p = dps[i];
                        if (p == null || !p.CanWrite) continue;

                        Type pt = p.PropertyType;
                        string pn = p.Name.ToLowerInvariant();

                        if (typeof(CharacterMainControl).IsAssignableFrom(pt) || pn.Contains("fromcharacter"))
                        {
                            if (mainChar != null)
                            {
                                p.SetValue(dmg, mainChar, null);
                            }
                            continue;
                        }

                        if (typeof(Component).IsAssignableFrom(pt) && pn.Contains("receiver"))
                        {
                            p.SetValue(dmg, receiver, null);
                            continue;
                        }

                        if (pt == typeof(Health) && targetHealth != null && pn.Contains("health"))
                        {
                            p.SetValue(dmg, targetHealth, null);
                            continue;
                        }

                        if ((pt == typeof(int) || pt == typeof(float)) &&
                            (pn.Contains("damage") || pn.Contains("dmg") || pn.Contains("amount")))
                        {
                            if (pt == typeof(int))
                                p.SetValue(dmg, 999999, null);
                            else
                                p.SetValue(dmg, 999999f, null);
                            continue;
                        }
                    }
                }
                catch (Exception exSet)
                {
                    Debug.Log("[DeathNote] TryInvokeDamageOnReceiver DamageInfo 셋업 예외: " + exSet);
                }

                Debug.Log("[DeathNote] KillViaDamageReceiver: " + rt.Name + "." + chosenMethod.Name +
                          "(" + damageInfoType.Name + ") 호출 시도");
                chosenMethod.Invoke(receiver, new object[] { dmg });
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] TryInvokeDamageOnReceiver 예외: " + ex);
                return false;
            }
        }

        // ───── Health.OnDead 수동 발사 ─────
        private void FireHealthOnDeadEvent(Health health)
        {
            if (health == null) return;

            try
            {
                Type t = typeof(Health);

                FieldInfo fi = t.GetField("OnDead",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi == null)
                {
                    Debug.Log("[DeathNote] FireHealthOnDeadEvent: Health.OnDead 필드를 찾지 못함.");
                    return;
                }

                Delegate del = fi.GetValue(null) as Delegate;
                if (del == null)
                {
                    Debug.Log("[DeathNote] FireHealthOnDeadEvent: Health.OnDead에 리스너 없음.");
                    return;
                }

                MethodInfo invoke = del.GetType().GetMethod("Invoke");
                if (invoke == null)
                {
                    Debug.Log("[DeathNote] FireHealthOnDeadEvent: Invoke 메서드를 찾지 못함.");
                    return;
                }

                ParameterInfo[] ps = invoke.GetParameters();
                object[] args = new object[ps.Length];

                for (int i = 0; i < ps.Length; i++)
                {
                    Type pt = ps[i].ParameterType;

                    if (typeof(Health).IsAssignableFrom(pt))
                    {
                        args[i] = health;
                    }
                    else if (pt == typeof(bool))
                    {
                        args[i] = true;
                    }
                    else if (pt == typeof(int))
                    {
                        args[i] = 1;
                    }
                    else if (pt == typeof(float))
                    {
                        args[i] = 1f;
                    }
                    else
                    {
                        try
                        {
                            args[i] = Activator.CreateInstance(pt);
                        }
                        catch
                        {
                            args[i] = null;
                        }
                    }
                }

                Debug.Log("[DeathNote] FireHealthOnDeadEvent: Health.OnDead 수동 호출 시도");
                del.DynamicInvoke(args);
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] FireHealthOnDeadEvent 예외: " + ex);
            }
        }

        // ───── 보스에 붙은 Die/OnDead 메서드 호출 ─────
        private bool CallDeathMethodsOnAttachedComponents(Component target, Health targetHealth)
        {
            if (target == null) return false;
            GameObject go = target.gameObject;
            if (go == null) return false;

            bool called = false;

            try
            {
                MonoBehaviour[] comps = go.GetComponentsInChildren<MonoBehaviour>(true);
                if (comps == null || comps.Length == 0)
                    return false;

                for (int i = 0; i < comps.Length; i++)
                {
                    MonoBehaviour mb = comps[i];
                    if (mb == null) continue;

                    Type t = mb.GetType();
                    MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                    for (int m = 0; m < methods.Length; m++)
                    {
                        MethodInfo mi = methods[m];
                        if (mi == null) continue;
                        if (mi.IsSpecialName) continue;

                        ParameterInfo[] ps = mi.GetParameters();
                        string ln = mi.Name.ToLowerInvariant();

                        bool looksLikeDeath =
                            ln.Contains("die") ||
                            ln.Contains("dead") ||
                            ln.Contains("death") ||
                            ln.Contains("ondead") ||
                            ln.Contains("ondeath") ||
                            ln.Contains("kill");

                        if (!looksLikeDeath)
                            continue;

                        object[] args = null;

                        if (ps == null || ps.Length == 0)
                        {
                            args = null;
                        }
                        else if (ps.Length == 1)
                        {
                            Type pt = ps[0].ParameterType;
                            object arg = null;

                            if (pt == typeof(bool))
                            {
                                arg = true;
                            }
                            else if (pt == typeof(int))
                            {
                                arg = 1;
                            }
                            else if (pt == typeof(float))
                            {
                                arg = 1f;
                            }
                            else if (typeof(Health).IsAssignableFrom(pt) && targetHealth != null)
                            {
                                arg = targetHealth;
                            }
                            else
                            {
                                try
                                {
                                    arg = Activator.CreateInstance(pt);
                                }
                                catch
                                {
                                    arg = null;
                                }
                            }

                            args = new object[] { arg };
                        }
                        else
                        {
                            continue;
                        }

                        try
                        {
                            Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents: " +
                                      t.Name + "." + mi.Name + " 호출");
                            mi.Invoke(mb, args);
                            called = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents 예외(" +
                                      t.Name + "." + mi.Name + "): " + ex);
                        }
                    }
                }
            }
            catch (Exception exOuter)
            {
                Debug.Log("[DeathNote] CallDeathMethodsOnAttachedComponents 전체 예외: " + exOuter);
            }

            return called;
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

                MonoBehaviour[] mbs = go.GetComponentsInChildren<MonoBehaviour>(true);
                if (mbs != null)
                {
                    for (int i = 0; i < mbs.Length; i++)
                    {
                        MonoBehaviour mb = mbs[i];
                        if (mb == null) continue;

                        Type t = mb.GetType();
                        string ln = t.Name.ToLowerInvariant();

                        bool looksLikeMover =
                            ln.Contains("navmesh") ||
                            ln.Contains("agent") ||
                            ln.Contains("ai") ||
                            ln.Contains("move") ||
                            ln.Contains("locomotion");

                        if (!looksLikeMover)
                            continue;

                        try
                        {
                            bool disabledByReflection = false;

                            PropertyInfo pEnabled = t.GetProperty("enabled", BINDING_FLAGS);
                            if (pEnabled != null && pEnabled.CanWrite && pEnabled.PropertyType == typeof(bool))
                            {
                                pEnabled.SetValue(mb, false, null);
                                disabledByReflection = true;
                            }
                            else
                            {
                                FieldInfo fEnabled = t.GetField("enabled", BINDING_FLAGS);
                                if (fEnabled != null && fEnabled.FieldType == typeof(bool))
                                {
                                    fEnabled.SetValue(mb, false);
                                    disabledByReflection = true;
                                }
                            }

                            if (!disabledByReflection)
                            {
                                mb.enabled = false;
                            }

                            Debug.Log("[DeathNote] FreezeCorpse: mover/AI disabled - " + t.FullName);
                        }
                        catch (Exception exMover)
                        {
                            Debug.Log("[DeathNote] FreezeCorpse mover disable 예외(" + t.FullName + "): " + exMover);
                        }
                    }
                }
            }
            catch (Exception exOuter)
            {
                Debug.Log("[DeathNote] FreezeCorpseComponents 전체 예외: " + exOuter);
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

                // 프로퍼티
                PropertyInfo[] props = t.GetProperties(BINDING_FLAGS);
                for (int i = 0; i < props.Length; i++)
                {
                    PropertyInfo p = props[i];
                    if (p == null || !p.CanRead || !p.CanWrite) continue;

                    Type pt = p.PropertyType;
                    string n = p.Name.ToLowerInvariant();

                    if (pt == typeof(bool))
                    {
                        object v = p.GetValue(health, null);
                        bool cur = v is bool b && b;

                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            if (!cur) p.SetValue(health, true, null);
                        }
                        else if (n.Contains("alive"))
                        {
                            if (cur) p.SetValue(health, false, null);
                        }
                    }
                    else if (pt == typeof(int))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            object v = p.GetValue(health, null);
                            int cur = (v is int) ? (int)v : 0;
                            if (cur > 0) p.SetValue(health, -1, null);
                        }
                    }
                    else if (pt == typeof(float))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            object v = p.GetValue(health, null);
                            float cur = (v is float) ? (float)v : 0f;
                            if (cur > 0f) p.SetValue(health, -1f, null);
                        }
                    }
                }

                // 필드
                FieldInfo[] fields = t.GetFields(BINDING_FLAGS);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if (f == null) continue;

                    Type ft = f.FieldType;
                    string n = f.Name.ToLowerInvariant();

                    if (ft == typeof(bool))
                    {
                        object v = f.GetValue(health);
                        bool cur = v is bool b && b;

                        if (n.Contains("dead") || n.Contains("isdead") || n.Contains("is_dead"))
                        {
                            if (!cur) f.SetValue(health, true);
                        }
                        else if (n.Contains("alive"))
                        {
                            if (cur) f.SetValue(health, false);
                        }
                    }
                }

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if (f == null) continue;

                    Type ft = f.FieldType;
                    string n = f.Name.ToLowerInvariant();

                    if (ft == typeof(int))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            object v = f.GetValue(health);
                            int cur = (v is int) ? (int)v : 0;
                            if (cur > 0) f.SetValue(health, -1);
                        }
                    }
                    else if (ft == typeof(float))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            object v = f.GetValue(health);
                            float cur = (v is float) ? (float)v : 0f;
                            if (cur > 0f) f.SetValue(health, -1f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] EnsureHealthStaysDead 예외: " + ex);
            }
        }

        // ───── Health 강제 킬 ─────
        private bool KillByHealthReflection(Health health)
        {
            if (health == null) return false;

            Type t = health.GetType();
            bool changed = false;

            // 1) Kill/Die/Death/Dead 메서드(파라미터 없음)
            try
            {
                MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m == null) continue;

                    if (m.IsSpecialName)
                        continue;

                    if (m.GetParameters().Length != 0)
                        continue;

                    string ln = m.Name.ToLowerInvariant();
                    if (ln.Contains("kill") || ln.Contains("die") || ln.Contains("death") || ln.Contains("dead"))
                    {
                        Debug.Log("[DeathNote] KillByHealthReflection: 메서드 호출 -> " + t.Name + "." + m.Name + "()");
                        m.Invoke(health, null);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] KillByHealthReflection direct method 예외: " + ex);
            }

            // 2) Damage/Hit(숫자 1개) 메서드
            try
            {
                MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m == null) continue;

                    if (m.IsSpecialName)
                        continue;

                    ParameterInfo[] ps = m.GetParameters();
                    if (ps == null || ps.Length != 1)
                        continue;

                    string ln = m.Name.ToLowerInvariant();
                    if (!(ln.Contains("damage") || ln.Contains("hit")))
                        continue;

                    Type pt = ps[0].ParameterType;
                    object big;
                    if (pt == typeof(int))
                        big = 999999;
                    else if (pt == typeof(float))
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

            // 3) 프로퍼티/필드 강제 조작
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
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 프로퍼티 true -> " + t.Name + "." + p.Name);
                        }
                        else if (n.Contains("alive"))
                        {
                            p.SetValue(health, false, null);
                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 프로퍼티 false -> " + t.Name + "." + p.Name);
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
                            Debug.Log("[DeathNote] KillByHealthReflection: hp/health 프로퍼티 -1 -> " + t.Name + "." + p.Name);
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
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 필드 true -> " + t.Name + "." + f.Name);
                        }
                        else if (n.Contains("alive"))
                        {
                            f.SetValue(health, false);
                            changed = true;
                            Debug.Log("[DeathNote] KillByHealthReflection: bool 필드 false -> " + t.Name + "." + f.Name);
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
                            Debug.Log("[DeathNote] KillByHealthReflection: hp/health 필드 -1 -> " + t.Name + "." + f.Name);
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

        // ───── CharacterMainControl 강제 킬 ─────
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

                    if (m.IsSpecialName)
                        continue;

                    if (m.GetParameters().Length != 0)
                        continue;

                    string ln = m.Name.ToLowerInvariant();
                    if (ln.Contains("kill") || ln.Contains("die") || ln.Contains("death") || ln.Contains("dead"))
                    {
                        Debug.Log("[DeathNote] KillByCharacterReflection: 메서드 호출 -> " + t.Name + "." + m.Name + "()");
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
                MethodInfo[] methods = t.GetMethods(BINDING_FLAGS);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m == null) continue;

                    if (m.IsSpecialName)
                        continue;

                    ParameterInfo[] ps = m.GetParameters();
                    if (ps == null || ps.Length != 1)
                        continue;

                    string ln = m.Name.ToLowerInvariant();
                    if (!(ln.Contains("damage") || ln.Contains("hit")))
                        continue;

                    Type pt = ps[0].ParameterType;
                    object big;
                    if (pt == typeof(int))
                        big = 999999;
                    else if (pt == typeof(float))
                        big = 999999f;
                    else
                        continue;

                    Debug.Log("[DeathNote] KillByCharacterReflection: 데미지 메서드 호출 -> " +
                              t.Name + "." + m.Name + "(" + big + ")");
                    m.Invoke(character, new object[] { big });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[DeathNote] KillByCharacterReflection damage method 예외: " + ex);
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
                            Debug.Log("[DeathNote] KillByCharacterReflection: bool 프로퍼티 true -> " + t.Name + "." + p.Name);
                        }
                        else if (n.Contains("alive"))
                        {
                            p.SetValue(character, false, null);
                            changed = true;
                            Debug.Log("[DeathNote] KillByCharacterReflection: bool 프로퍼티 false -> " + t.Name + "." + p.Name);
                        }
                    }
                    else if (pt == typeof(int) || pt == typeof(float))
                    {
                        if ((n.Contains("hp") || n.Contains("health") || n.Contains("current")) &&
                            !n.Contains("max") && !n.Contains("hash") && !n.Contains("height"))
                        {
                            if (pt == typeof(int))
                                p.SetValue(character, -1, null);
                            else
                                p.SetValue(character, -1f, null);

                            changed = true;
                            Debug.Log("[DeathNote] KillByCharacterReflection: hp/health 프로퍼티 -1 -> " + t.Name + "." + p.Name);
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
                            Debug.Log("[DeathNote] KillByCharacterReflection: bool 필드 true -> " + t.Name + "." + f.Name);
                        }
                        else if (n.Contains("alive"))
                        {
                            f.SetValue(character, false);
                            changed = true;
                            Debug.Log("[DeathNote] KillByCharacterReflection: bool 필드 false -> " + t.Name + "." + f.Name);
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
                                f.SetValue(character, -1);
                            else
                                f.SetValue(character, -1f);

                            changed = true;
                            Debug.Log("[DeathNote] KillByCharacterReflection: hp/health 필드 -1 -> " + t.Name + "." + f.Name);
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
    }
}
