

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using LibMVCS = XTC.FMP.LIB.MVCS;
using XTC.FMP.MOD.GridMenu.LIB.Proto;
using XTC.FMP.MOD.GridMenu.LIB.MVCS;
using System;
using UnityEngine.Video;
using System.Collections;

namespace XTC.FMP.MOD.GridMenu.LIB.Unity
{
    /// <summary>
    /// 实例类
    /// </summary>
    public class MyInstance : MyInstanceBase
    {
        public class UiReference
        {
            public RectTransform rtCellTemplate;
        }

        private UiReference uiReference_ = new UiReference();

        public MyInstance(string _uid, string _style, MyConfig _config, MyCatalog _catalog, LibMVCS.Logger _logger, Dictionary<string, LibMVCS.Any> _settings, MyEntryBase _entry, MonoBehaviour _mono, GameObject _rootAttachments)
            : base(_uid, _style, _config, _catalog, _logger, _settings, _entry, _mono, _rootAttachments)
        {
        }

        /// <summary>
        /// 当被创建时
        /// </summary>
        /// <remarks>
        /// 可用于加载主题目录的数据
        /// </remarks>
        public void HandleCreated()
        {
            uiReference_.rtCellTemplate = rootUI.transform.Find("templateCell").GetComponent<RectTransform>();
            uiReference_.rtCellTemplate.gameObject.SetActive(false);
            loadTextureFromTheme(style_.background.image, (_texture) =>
            {
                var imgBackground = rootUI.transform.Find("imgBackground").GetComponent<RawImage>();
                imgBackground.texture = _texture;
            }, () => { });

            createCellS();

            rootUI.transform.Find("debug_imgGrid").gameObject.SetActive(style_.debug.active);
            if (style_.debug.active)
                drawDebugGridLine();
        }

        /// <summary>
        /// 当被删除时
        /// </summary>
        public void HandleDeleted()
        {
        }

        /// <summary>
        /// 当被打开时
        /// </summary>
        /// <remarks>
        /// 可用于加载内容目录的数据
        /// </remarks>
        public void HandleOpened(string _source, string _uri)
        {
            rootUI.gameObject.SetActive(true);
            rootWorld.gameObject.SetActive(true);
        }

        /// <summary>
        /// 当被关闭时
        /// </summary>
        public void HandleClosed()
        {
            rootUI.gameObject.SetActive(false);
            rootWorld.gameObject.SetActive(false);
        }

        private void createCellS()
        {

            var rtRootUI = rootUI.GetComponent<RectTransform>();
            int rootWidth = (int)rtRootUI.rect.width;
            int rootHeight = (int)rtRootUI.rect.height;
            int gridWidth = rootWidth / style_.grid.column;
            int gridHeight = style_.grid.row == 0 ? gridWidth : rootHeight / style_.grid.row;
            Action<GameObject, string> buildRawImage = (_obj, _image) =>
            {
                var img = _obj.AddComponent<RawImage>();
                if (!string.IsNullOrEmpty(_image))
                {
                    loadTextureFromTheme(_image, (_texture) =>
                    {
                        img.texture = _texture;
                    }, () => { });
                }
            };

            Action<GameObject, string, Vector4> buildSlicedImage = (_obj, _image, _border) =>
            {
                var img = _obj.AddComponent<Image>();
                img.type = Image.Type.Sliced;
                if (!string.IsNullOrEmpty(_image))
                {
                    loadTextureFromTheme(_image, (_texture) =>
                    {
                        img.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, _border);
                    }, () => { });
                }
            };


            Action<GameObject, string> buildVideo = (_obj, _uri) =>
            {
                var vp = _obj.AddComponent<VideoPlayer>();
                vp.source = VideoSource.Url;
                if (!string.IsNullOrEmpty(_uri))
                {
                    vp.url = _uri;
                }
                var size = _obj.GetComponent<RectTransform>().rect.size;
                RenderTexture renderTexture = new RenderTexture((int)size.x, (int)size.y, 32);
                vp.targetTexture = renderTexture;
                vp.playOnAwake = false;

                var image = _obj.AddComponent<RawImage>();
                image.texture = renderTexture;
            };
            foreach (var cell in style_.cellS)
            {
                int x = (cell.columnStart - 1) * gridWidth;
                int width = (cell.columnEnd + 1 - cell.columnStart) * gridWidth;
                int y = -((cell.rowStart - 1) * gridHeight);
                int height = (cell.rowEnd + 1 - cell.rowStart) * gridHeight;
                var cloneCell = GameObject.Instantiate(uiReference_.rtCellTemplate.gameObject, uiReference_.rtCellTemplate.parent);
                cloneCell.name = cell.name;
                var rtCloneCell = cloneCell.GetComponent<RectTransform>();
                rtCloneCell.anchoredPosition = new Vector2(x, y);
                rtCloneCell.sizeDelta = new Vector2(width, height);
                cloneCell.SetActive(true);
                if (null == cell.content)
                    continue;

                if (cell.content.type == "RawImage")
                {
                    var imageValue = tryParseFromParameter<string>(cell.content.parameterS, "image");
                    buildRawImage(cloneCell, imageValue);
                }
                else if (cell.content.type == "SlicedImage")
                {
                    var imageValue = tryParseFromParameter<string>(cell.content.parameterS, "image");
                    var border_left = tryParseFromParameter<int>(cell.content.parameterS, "border_left");
                    var border_right = tryParseFromParameter<int>(cell.content.parameterS, "border_right");
                    var border_top = tryParseFromParameter<int>(cell.content.parameterS, "border_top");
                    var border_bottom = tryParseFromParameter<int>(cell.content.parameterS, "border_bottom");
                    buildSlicedImage(cloneCell, imageValue, new Vector4(border_left, border_bottom, border_right, border_top));
                }
                else if (cell.content.type == "Button")
                {
                    var imageValue = tryParseFromParameter<string>(cell.content.parameterS, "image");
                    buildRawImage(cloneCell, imageValue);
                    var btn = cloneCell.AddComponent<Button>();
                    btn.targetGraphic = cloneCell.GetComponent<RawImage>();
                    btn.onClick.AddListener(() =>
                    {
                        Dictionary<string, object> variableS = new Dictionary<string, object>();
                        publishSubjects(cell.content.subjectS, variableS);
                    });
                }
                else if (cell.content.type == "ClickArea")
                {
                    var img = cloneCell.AddComponent<RawImage>();
                    img.color = new Color(0, 0, 0, 0);
                    var btn = cloneCell.AddComponent<Button>();
                    btn.transition = Selectable.Transition.None;
                    btn.onClick.AddListener(() =>
                    {
                        Dictionary<string, object> variableS = new Dictionary<string, object>();
                        publishSubjects(cell.content.subjectS, variableS);
                    });
                }
            }
        }

        private void drawDebugGridLine()
        {
            Color lineColor;
            if (!ColorUtility.TryParseHtmlString(style_.debug.lineColor, out lineColor))
            {
                lineColor = Color.white;
            }
            Color cellColor;
            if (!ColorUtility.TryParseHtmlString(style_.debug.cellColor, out cellColor))
            {
                cellColor = Color.white;
            }
            var rtGrid = rootUI.transform.Find("debug_imgGrid").GetComponent<RectTransform>();
            rtGrid.transform.SetAsLastSibling();
            int width = (int)rtGrid.rect.width;
            int height = (int)rtGrid.rect.height;
            int gridWidth = width / style_.grid.column;
            int gridHeight = style_.grid.row == 0 ? gridWidth : height / style_.grid.row;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    Color color = new Color(0, 0, 0, 0);
                    if (x % gridWidth == 0 || y % gridHeight == 0 && style_.debug.drawLine)
                        color = lineColor;
                    bool inCell = false;
                    foreach (var cell in style_.cellS)
                    {
                        if (inCell)
                            continue;
                        inCell = x >= (cell.columnStart - 1) * gridWidth &&
                            x < (cell.columnEnd) * gridWidth &&
                            y >= (cell.rowStart - 1) * gridHeight &&
                            y < (cell.rowEnd) * gridHeight;
                    }
                    if (inCell && style_.debug.drawCell)
                        color = cellColor;
                    texture.SetPixel(x, height - y, color);
                }
            }
            texture.Apply();
            rtGrid.GetComponent<RawImage>().texture = texture;
        }

        private Type tryParseFromParameter<Type>(MyConfigBase.Parameter[] _parameterS, string _key)
        {
            object value = null;
            foreach (var parameter in _parameterS)
            {
                if (parameter.key == _key)
                {
                    if (parameter.type == "string")
                        value = parameter.value;
                    else if (parameter.type == "int")
                        value = int.Parse(parameter.value);
                    else if (parameter.type == "float")
                        value = float.Parse(parameter.value);
                    else if (parameter.type == "bool")
                        value = bool.Parse(parameter.value);
                    break;
                }
            }
            return (Type)value;
        }
    }
}
