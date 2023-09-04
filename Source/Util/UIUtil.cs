// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UO = UnityEngine.Object;

namespace SomaSim.Util
{
    public static class UIUtil
    {
        //
        // creating things

        public static void EnsureChildCount<T> (this GameObject container, List<T> data, GameObject childTemplate)
            => EnsureChildCount(container, data.Count, childTemplate);

        public static void EnsureChildCount (this GameObject container, int target, GameObject childTemplate)
            => EnsureChildCount(container.transform, target, childTemplate);

        public static void EnsureChildCount (this Transform tr, int target, GameObject childTemplate) {
            int delta = tr.childCount - target;

            while (delta < 0) { // not enough? make more
                UO.Instantiate(childTemplate, tr);
                delta++;
            }

            while (delta > 0) { // too many? remove some
                var child = tr.GetChild(tr.childCount - 1);
                child.SetParent(null);
                UO.Destroy(child.gameObject);
                delta--;
            }
        }

        public static void InitializeChildren<T> (this GameObject container, List<T> data, Action<int, GameObject, T> initFunction) {
            var childCount = container.transform.childCount;

            var count = Math.Min(childCount, data.Count);
            for (int i = 0; i < count; i++) {
                var element = container.transform.GetChild(i);
                var datum = data[i];
                initFunction(i, element.gameObject, datum);
            }
        }



        //
        // destroying things

        public static void DestroyAllChildren (this GameObject go) =>
            DestroyAllChildren(go.transform);

        public static void DestroyAllChildren (this Transform tr) {
            while (tr.childCount > 0) {
                var childtr = tr.GetChild(tr.childCount - 1);
                childtr.SetParent(null);
                UO.Destroy(childtr.gameObject);
            }
        }

        public static void DestroyChildByName (this Transform container, string name) {
            var child = container.Find(name);
            if (child != null) { UO.Destroy(child.gameObject); }
        }

        public static void DestroyAllChildrenInEditMode (this GameObject go) =>
            DestroyAllChildrenInEditMode(go.transform);

        public static void DestroyAllChildrenInEditMode (this Transform tr) {
            while (tr.childCount > 0) {
                var childtr = tr.GetChild(tr.childCount - 1);
                childtr.SetParent(null);
                UO.DestroyImmediate(childtr.gameObject);
            }
        }

        public static void DestroyChildIfPresent (this GameObject go, int index) {
            if (index >= 0 && index < go.transform.childCount) {
                var childtr = go.transform.GetChild(index).transform;
                childtr.SetParent(null);
                UO.Destroy(childtr.gameObject);
            }
        }

        //
        // recursive search extensions

        public static void WalkSelfAndChildren (this GameObject go, Action<Transform> action, bool recursive = true) {
            action(go.transform);
            WalkChildren(go, action, recursive);
        }

        public static void WalkChildren (this GameObject go, Action<Transform> action, bool recursive = true) =>
            go.transform.WalkChildren(action, recursive);

        public static void WalkChildren (this Transform tr, Action<Transform> action, bool recursive = true) {
            foreach (Transform child in tr) {
                action.Invoke(child);
                if (recursive) {
                    child.WalkChildren(action);
                }
            }
        }

        public static void ForEachChild (this GameObject go, Action<int, GameObject> callback) {
            var tr = go.transform;
            for (int i = 0, count = tr.childCount; i < count; i++) {
                var child = tr.GetChild(i).gameObject;
                callback(i, child);
            }
        }

        public static void SetActiveOnlyOneChild (this GameObject go, string name) {
            var tr = go.transform;
            foreach (Transform child in tr) {
                child.gameObject.SetActive(false);
            }
            var matching = tr.Find(name);
            if (matching != null) {
                matching.gameObject.SetActive(true);
            }
        }

        public static GameObject FindGameObject (this Transform tr, string name, bool recursive) {
            var match = tr.Find(name);
            if (match != null) { return match.gameObject; }

            if (recursive) {
                foreach (Transform child in tr) {
                    var matchRecursive = child.FindGameObject(name, recursive);
                    if (matchRecursive != null) { return matchRecursive; }
                }
            }

            return null;
        }

        public static GameObject FindGameObject (this Transform tr, Predicate<Transform> predicate, bool recursive) {
            if (predicate(tr)) { return tr.gameObject; }

            if (recursive) {
                foreach (Transform child in tr) {
                    var matchRecursive = child.FindGameObject(predicate, recursive);
                    if (matchRecursive != null) { return matchRecursive; }
                }
            }

            return null;
        }

        public static IEnumerable<GameObject> FindAllGameObjects (this Transform tr, string name) {
            var matchCurrent = tr.Find(name);
            if (matchCurrent != null) {
                yield return matchCurrent.gameObject;
            }

            foreach (Transform child in tr) {
                var matchRecursive = child.FindAllGameObjects(name);
                foreach (var matchedChild in matchRecursive) {
                    yield return matchedChild;
                }
            }
        }

        public static bool IsCandidateInMyHierarchy (this Transform tr, GameObject candidate) =>
            FindGameObject(tr, tt => tt.gameObject == candidate, true);


        //
        // helper accessors

        public static bool IsInputOverUI => EventSystem.current.IsPointerOverGameObject();

        public static GameObject FindInputOverUIElementsOrNull (List<GameObject> elements) {
            PointerEventData eventData = new PointerEventData(EventSystem.current) {
                position = Input.mousePosition
            };
            using (var results = ListPool<RaycastResult>.Allocate()) {
                EventSystem.current.RaycastAll(eventData, results);
                foreach (var result in results) {
                    if (!result.isValid) { continue; }
                    var candidate = result.gameObject;

                    foreach (var element in elements) {
                        if (element.transform.IsCandidateInMyHierarchy(candidate)) {
                            return element;
                        }
                    }
                }
            }
            return null;
        }

        //
        // component extensions

        public static T GetOrAddComponent<T> (this GameObject obj) where T : Component {
            if (!obj.TryGetComponent<T>(out var comp)) {
                comp = obj.AddComponent<T>();
            }
            return comp;
        }

        public static void ProduceComponentsInChildren<T> (this GameObject go, List<T> results, List<T> temp) where T : Component {
            go.GetComponentsInChildren(temp);
            results.AddRange(temp);
            temp.Clear();
        }

        public static void ProduceComponentsInObjectAndChildren<T> (this GameObject go, List<T> results) where T : Component {
            using (var list = ListPool<T>.Allocate()) {
                // we have to go through a temp list because the calls to get components clear out whatever list we pass in

                go.GetComponents(list);
                results.AddRange(list);

                list.Clear(); // just in case unity changes this behavior in the future...

                go.GetComponentsInChildren(list);
                results.AddRange(list);
            }
        }

        public static T GetComponentInObjectOrParents<T> (this GameObject go) where T : Component {
            if (go == null) { return null; }

            if (go.TryGetComponent<T>(out var c)) { return c; }

            var parent = go.transform.parent;
            if (parent == null) { return null; }

            return GetComponentInObjectOrParents<T>(go.transform.parent.gameObject);
        }

        public static void DestroyComponentIfAdded<T> (this GameObject obj) where T : Component {
            T comp = obj.GetComponent<T>();
            if (comp != null) { UO.Destroy(comp); }
        }


        // ui child element accessors

        public static GameObject GetChild (this GameObject dialog, string path) {
            var result = dialog.transform.FindGameObject(path, true);
            return result;
        }

        public static T GetChild<T> (this GameObject dialog, string path) where T : Component {
            var child = GetChild(dialog, path);
            return (child == null) ? default : child.GetComponent<T>();
        }

        public static bool HasChildOfType<T> (this GameObject dialog, string path) where T : Component {
            var child = GetChild(dialog, path);
            return (child == null) ? false : child.TryGetComponent(out T _);
        }

        public static GameObject GetChildSafe (this GameObject dialog, int index) =>
            (index >= 0 && index < dialog.transform.childCount) ?
            dialog.transform.GetChild(index).gameObject :
            default;

        public static RectTransform GetRect (this GameObject dialog) =>
            dialog.GetComponent<RectTransform>();

        public static TextMeshProUGUI GetText (this GameObject dialog, string path) =>
            dialog.GetChild<TextMeshProUGUI>(path);

        public static Toggle GetToggle (this GameObject dialog, string path) =>
            dialog.GetChild<Toggle>(path);

        public static Image GetImage (this GameObject dialog, string path) =>
            dialog.GetChild<Image>(path);

        public static void SetImage (this GameObject dialog, string path, Sprite sprite) {
            var image = dialog.GetChild<Image>(path);
            if (image != null) { image.sprite = sprite; }
        }

        public static void SetImageOrHide (this GameObject dialog, string path, Sprite sprite) {
            var image = dialog.GetChild<Image>(path);
            if (image != null) {
                image.sprite = sprite;
                image.gameObject.SetActive(sprite != null);
            }
        }

        public static TMP_Dropdown GetDropdown (this GameObject dialog, string path) => dialog.GetChild<TMP_Dropdown>(path);

        public static Button GetButton (this GameObject dialog, string path) => dialog.GetChild<Button>(path);

        public static Slider GetSlider (this GameObject dialog, string path) => dialog.GetChild<Slider>(path);

        public static Selectable GetSelectable (this GameObject dialog, string path) => dialog.GetChild<Selectable>(path);

        public static void SetButtonListener (this GameObject dialog, string path, Action listener) {
            var comp = dialog.GetChild<Button>(path);
            if (comp != null) { comp.onClick.SetListener(() => { listener(); }); }
        }

        public static void SetSliderListener (this GameObject dialog, string path, Action<float> listener) {
            var comp = dialog.GetChild<Slider>(path);
            if (comp != null) { comp.onValueChanged.SetListener((float value) => { listener(value); }); }
        }

        public static void SetToggleListener (this GameObject dialog, string path, Action<bool> listener) {
            var comp = dialog.GetChild<Toggle>(path);
            if (comp != null) { comp.onValueChanged.SetListener((isOn) => { listener(isOn); }); }
        }

        public static void ClearButtonListeners (this GameObject dialog, string path) {
            var comp = dialog.GetChild<Button>(path);
            if (comp != null) { comp.onClick.RemoveAllListeners(); }
        }

        public static void SetButtonInteractable (this GameObject dialog, bool value, bool dimtext = true) {
            if (dialog.TryGetComponent<Button>(out var button)) {
                button.interactable = value;
                MaybeUpdateTextAlpha(button, value, dimtext);
            }
        }

        public static void SetButtonInteractable (this GameObject dialog, string path, bool value, bool dimtext = true) {
            if (dialog.HasChildOfType<Button>(path)) {
                var button = dialog.GetChild<Button>(path);
                button.interactable = value;
                MaybeUpdateTextAlpha(button, value, dimtext);
            }
        }

        private static void MaybeUpdateTextAlpha (Component c, bool value, bool dimtext) {
            using (var list = ListPool<TextMeshProUGUI>.Allocate()) {
                c.GetComponentsInChildren(true, list);
                foreach (var text in list) {
                    if (text != null) { text.alpha = (!value && dimtext) ? 0.5f : 1f; }
                }
            }
        }

        public static TextMeshProUGUI GetText (this GameObject dialog) => dialog.GetComponent<TextMeshProUGUI>();

        public static void SetText (this GameObject dialog, string path, string text) {
            var tm = dialog.GetChild<TextMeshProUGUI>(path);

            SetTextHelper(tm, text);
        }

        public static void SetTextOrHide (this GameObject dialog, string path, string text) {
            var tm = dialog.GetChild<TextMeshProUGUI>(path);
            if (tm != null) {
                bool show = !string.IsNullOrWhiteSpace(text);
                tm.gameObject.SetActive(show);
                if (show) { SetTextHelper(tm, text); }
            }
        }

        private static void SetTextHelper (TextMeshProUGUI tm, string text) {
            if (tm == null) { return; }

            tm.SetText(text);

            bool isLink = text.Contains("<link");
            LinkUtil.AddOrRemoveHandler(isLink, tm, true);
        }

        public static TextMeshProUGUI GetChildText (this GameObject dialog) => dialog.GetComponentInChildren<TextMeshProUGUI>();
        public static void SetChildText (this GameObject dialog, string text) => dialog.GetComponentInChildren<TextMeshProUGUI>().SetText(text);
        public static void SetChildText (this GameObject dialog, string path, string text) {
            var child = dialog.GetChild(path);
            if (child != null) {
                child.GetComponentInChildren<TextMeshProUGUI>().SetText(text);
            }
        }

        public static Button GetButton (this GameObject dialog) => dialog.GetComponent<Button>();

        public static Button GetChildButton (this GameObject dialog) => dialog.GetComponentInChildren<Button>();

        public static Toggle GetToggle (this GameObject dialog) => dialog.GetComponent<Toggle>();

        public static Image GetImage (this GameObject dialog) => dialog.GetComponent<Image>();
        public static Image GetChildImage (this GameObject dialog) => dialog.GetComponentInChildren<Image>();

        public static TMP_Dropdown GetDropdown (this GameObject dialog) => dialog.GetComponent<TMP_Dropdown>();

        public static void SetImageAlpha (this GameObject dialog, float alpha) {
            var image = dialog.GetImage();
            var old = image.color;
            image.color = new Color(old.r, old.g, old.b, alpha);
        }

        public static void SetActive (this GameObject dialog, string path, bool value) =>
            dialog.GetChild(path).SetActive(value);

        public static void SetUIElementWidth (this GameObject ui, float width) {
            var rect = ui.GetRect();
            rect.sizeDelta = rect.sizeDelta.NewX(width);
        }

        public static void SetUIElementHeight (this GameObject ui, float height) {
            var rect = ui.GetRect();
            rect.sizeDelta = rect.sizeDelta.NewY(height);
        }

        public static void SetUIElementY (this GameObject ui, float y) {
            var rect = ui.GetRect();
            rect.anchoredPosition = rect.anchoredPosition.NewY(y);
        }

        public static void SetUIElementX (this GameObject ui, float x) {
            var rect = ui.GetRect();
            rect.anchoredPosition = rect.anchoredPosition.NewX(x);
        }

        public static void ResetScrollView (this GameObject scrollView, bool toStart = true) {
            var scroll = scrollView.GetComponent<ScrollRect>();
            var y = toStart ? 1f : 0f;
            if (scroll != null) { scroll.normalizedPosition = new Vector2(0, y); }
        }

        public static void ResetAllChildScrollViews (this GameObject go, bool toStart = true) {
            var scrollViews = go.GetComponentsInChildren<ScrollRect>();
            var y = toStart ? 1f : 0f;
            foreach (var scroll in scrollViews) { scroll.normalizedPosition = new Vector2(0, y); }
        }


        public static void ForceRebuildLayoutImmediate (this GameObject go) {
            var rect = go.GetRect();
            if (rect != null) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }
    }

    [Serializable]
    public class DropdownOptionsItem<T> : TMP_Dropdown.OptionData
    {
        [SerializeField]
        public T data;

        public DropdownOptionsItem (string text, T data = default) : base(text) {
            this.data = data;
        }
    }

    public static class DropdownUtil
    {
        public static void SetOptions (this TMP_Dropdown dd, params string[] names) =>
            SetOptions(dd, names.ToList());

        public static void SetOptions (this TMP_Dropdown dd, List<string> names) {
            dd.ClearOptions();
            dd.AddOptions(names);
        }

        public static void SetOptions<T> (this TMP_Dropdown dd, List<string> names, List<T> values) =>
            SetOptions(dd, ListGenerators.Zip(names, values));

        public static void SetOptions<T> (this TMP_Dropdown dd, List<(string name, T value)> entries) {
            dd.ClearOptions();
            foreach (var (name, value) in entries) { dd.options.Add(new DropdownOptionsItem<T>(name, value)); }
            dd.RefreshShownValue();
        }

        public static void AddOptionsItem (this TMP_Dropdown dd, string text) =>
            AddOptionsItem(dd, text, text);

        public static void AddOptionsItem<T> (this TMP_Dropdown dd, string text, T data = default) {
            dd.options.Add(new DropdownOptionsItem<T>(text, data));
            dd.RefreshShownValue();
        }

        public static DropdownOptionsItem<T> GetOptionsItem<T> (this TMP_Dropdown dd, int i) {
            if (i < 0 || i >= dd.options.Count) { return null; }
            return dd.options[i] as DropdownOptionsItem<T>;
        }

        public static DropdownOptionsItem<T> GetCurrentOptionsItem<T> (this TMP_Dropdown dd) =>
            GetOptionsItem<T>(dd, dd.value);

        public static T GetCurrentOptionsItemData<T> (this TMP_Dropdown dd) =>
            GetOptionsItem<T>(dd, dd.value).data;
    }


    //
    // embedded hyperlinks

    public static class LinkUtil
    {
        public static void AddOrRemoveHandler (bool shouldHave, TextMeshProUGUI tm, bool wasTextReplaced) {
            if (tm == null) { return; }

            var linkHandler = tm.gameObject.GetComponent<LinkHandler>();

            if (shouldHave && linkHandler == null) {
                linkHandler = tm.gameObject.GetOrAddComponent<LinkHandler>();
            }
            if (shouldHave && linkHandler != null) {
                linkHandler.ResetLastLink(wasTextReplaced);
                linkHandler.ColorizeLinks(wasTextReplaced);
            }
            if (!shouldHave && linkHandler != null) {
                GameObject.Destroy(linkHandler);
            }
        }

        public static void SetAllLinksToColor (GameObject go, Color color) {
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null) { return; }

            foreach (var linkInfo in text.textInfo.linkInfo) {
                SetLinkToColor(linkInfo, color);
            }
        }

        public static void SetLinkToColor (TMP_LinkInfo linkInfo, Color32 color) {
            if (linkInfo.textComponent == null) { return; } // what even

            var textInfo = linkInfo.textComponent.textInfo;
            if (textInfo == null) { return; } // got lost somewhere

            for (int i = 0; i < linkInfo.linkTextLength; i++) {
                int charIndex = linkInfo.linkTextfirstCharacterIndex + i;
                if (charIndex >= textInfo.characterInfo.Length) {
                    break; // text field got updated behind our back
                }

                var charInfo = textInfo.characterInfo[charIndex];

                if (charInfo.isVisible) {
                    int meshIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;

                    Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
                    if (vertexIndex + 3 >= vertexColors.Length) { break; } // ibid.

                    vertexColors[vertexIndex + 0] = color;
                    vertexColors[vertexIndex + 1] = color;
                    vertexColors[vertexIndex + 2] = color;
                    vertexColors[vertexIndex + 3] = color;
                }
            }

            linkInfo.textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }

    //
    // Unity kludge
    public static class UnityKludge
    {
        /// <summary>
        /// Kluge for bug 1009730 regression in Unity 2017.4 which they don't want to fix FFS
        /// https://issuetracker.unity3d.com/issues/calling-transform-dot-setparents-resets-localposition-of-the-ui-gameobject
        /// </summary>
        public static void KlugeGoddamnUnity2017LocalPositionBug (this GameObject go) {
            var rect = go.GetComponent<RectTransform>();
            if (rect != null) {
                rect.ForceUpdateRectTransforms();
            }
        }

        /// <summary>
        /// Kluge for bug 1009730 regression in Unity 2017.4 which they don't want to fix FFS
        /// https://issuetracker.unity3d.com/issues/calling-transform-dot-setparents-resets-localposition-of-the-ui-gameobject
        /// </summary>
        public static void KlugeGoddamnUnity2017LocalPositionBug (this RectTransform rect) {
            if (rect != null) {
                rect.ForceUpdateRectTransforms();
            }
        }
    }

    public class LinkHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        //
        // this is a weaksauce polling interface, but this library knows nothing about game code
        // so it can't hook up to appropriate events. instead, it relies on someone polling as needed.

        private static List<string> _linkIDQueue = new List<string>();

        public static bool HasAnyLinkID () =>
            _linkIDQueue.Count > 0;

        public static string PeekLinkID () =>
            _linkIDQueue.Count > 0 ? _linkIDQueue[0] : null;

        public static void PushLinkID (string link) =>
            // push at the end
            _linkIDQueue.Add(link);

        public static string PopLinkID () =>
            // pop from the front
            _linkIDQueue.Count > 0 ? _linkIDQueue.RemoveAndReturn(0) : null;


        //
        // internal link mouse events and recoloring handlers

        internal (int index, TMP_LinkInfo linkInfo) FindTMPLinkUnderMouse () {
            var text = gameObject.GetComponent<TextMeshProUGUI>();
            if (text == null) { return (-1, default); }

            int index = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            if (index < 0) { return (-1, default); }

            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[index];
            return (index, linkInfo);
        }

        private readonly string StyledOpenLink = "<u><link=";
        private readonly string StyledCloseLink = "/link></u>";

        private readonly Color ColorPlain = new Color32(255, 255, 255, 255);
        private readonly Color ColorHover = new Color32(250, 185, 50, 255);
        private readonly Color ColorClick = new Color32(190, 145, 50, 255);

        private int _lastIndex = -1;
        private TMP_LinkInfo _lastLink = default;

        internal void ColorizeLinks (bool wasTextReplaced) {
            var tm = gameObject.GetComponent<TextMeshProUGUI>();
            var msg = tm.text.Replace("<link=", StyledOpenLink).Replace("/link>", StyledCloseLink);
            tm.SetText(msg);

            if (!wasTextReplaced) {
                LinkUtil.SetAllLinksToColor(gameObject, ColorPlain);
            }
        }

        internal void ResetLastLink (bool wasTextReplaced) {
            if (_lastIndex < 0) { return; }

            if (!wasTextReplaced) {
                LinkUtil.SetLinkToColor(_lastLink, ColorPlain);
            }

            _lastLink = default;
            _lastIndex = -1;
        }

        private void SetLastLink (int index, TMP_LinkInfo linkInfo) {
            if (index < 0) { return; }
            LinkUtil.SetLinkToColor(linkInfo, ColorHover);
            _lastLink = linkInfo;
            _lastIndex = index;
        }

        public void Update () {
            var (index, linkInfo) = FindTMPLinkUnderMouse();
            if (index != _lastIndex) {
                // uncolorize last link if it exists
                ResetLastLink(false);
                // recolorize the new link if it exists
                SetLastLink(index, linkInfo);
            }
        }

        public void OnPointerDown (PointerEventData _) {
            if (_lastIndex >= 0) {
                LinkUtil.SetLinkToColor(_lastLink, ColorClick);
            }
        }

        public void OnPointerUp (PointerEventData _) {
            if (_lastIndex >= 0) {
                LinkUtil.SetLinkToColor(_lastLink, ColorHover);
                PushLinkID(_lastLink.GetLinkID());
                ResetLastLink(true);
            }
        }
    }

}
