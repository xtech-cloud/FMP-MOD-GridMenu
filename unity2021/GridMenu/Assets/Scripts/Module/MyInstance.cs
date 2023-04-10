

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
        public class CarouselTask
        {
            public string type = "";
            public string uri = "";
            public int duration = 0;
        }

        public class CarouselTaskPool
        {
            public int timer = 0;
            public int activeIndex = 0;
            public GameObject gameObject;
            public List<CarouselTask> taskS = new List<CarouselTask>();
        }

        public class UiReference
        {
            public RectTransform rtCellTemplate;
        }

        private UiReference uiReference_ = new UiReference();
        private bool isOpened_ = false;
        /// <summary>
        /// key：gameobject的instanceId
        /// </summary>
        private Dictionary<int, CarouselTaskPool> carouselTaskPoolS_ = new Dictionary<int, CarouselTaskPool>();

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
            isOpened_ = true;


            foreach (var pair in carouselTaskPoolS_)
            {
                carouselTaskPoolS_[pair.Key].activeIndex = 0;
                carouselTaskPoolS_[pair.Key].timer = 0;
                refreshCarousel(pair.Key);
            }
            mono_.StartCoroutine(updateCarouselTick());
        }

        /// <summary>
        /// 当被关闭时
        /// </summary>
        public void HandleClosed()
        {
            rootUI.gameObject.SetActive(false);
            rootWorld.gameObject.SetActive(false);
            isOpened_ = false;
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
                else if (cell.content.type == "Carousel")
                {
                    buildVideo(cloneCell, "");
                    int instanceID = cloneCell.GetInstanceID();
                    if (!carouselTaskPoolS_.ContainsKey(instanceID))
                    {
                        carouselTaskPoolS_[instanceID] = new CarouselTaskPool();
                        carouselTaskPoolS_[instanceID].gameObject = cloneCell;
                    }
                    var count = tryParseFromParameter<int>(cell.content.parameterS, "count");
                    for (int i = 0; i < count; i++)
                    {
                        var carouselTask = new CarouselTask();
                        carouselTask.type = tryParseFromParameter<string>(cell.content.parameterS, string.Format("item_{0}_type", i + 1));
                        carouselTask.uri = tryParseFromParameter<string>(cell.content.parameterS, string.Format("item_{0}_uri", i + 1));
                        carouselTask.duration = tryParseFromParameter<int>(cell.content.parameterS, string.Format("item_{0}_duration", i + 1));
                        carouselTaskPoolS_[instanceID].taskS.Add(carouselTask);
                    }
                }
                cloneCell.SetActive(true);
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

        private IEnumerator updateCarouselTick()
        {
            while (isOpened_)
            {
                foreach (var pair in carouselTaskPoolS_)
                {
                    var taskPool = carouselTaskPoolS_[pair.Key];
                    var task = taskPool.taskS[taskPool.activeIndex];
                    if (taskPool.timer >= task.duration)
                    {
                        taskPool.timer = 0;
                        taskPool.activeIndex += 1;
                        if (taskPool.activeIndex >= taskPool.taskS.Count)
                            taskPool.activeIndex = 0;
                        refreshCarousel(pair.Key);
                    }
                    taskPool.timer += 1;
                }
                yield return new WaitForSeconds(1);
            }
        }

        private void refreshCarousel(int _gameObejctID)
        {
            var taskPool = carouselTaskPoolS_[_gameObejctID];
            var task = taskPool.taskS[taskPool.activeIndex];
            if (task.type == "Image")
            {
                loadTextureFromTheme(task.uri, (_texture) =>
                {
                    taskPool.gameObject.GetComponent<RawImage>().texture = _texture;
                }, () => { });
            }
            else if (task.type == "Video")
            {
                string path = settings_["path.themes"].AsString();
                path = System.IO.Path.Combine(path, MyEntryBase.ModuleName);
                string filefullpath = System.IO.Path.Combine(path, task.uri);
                var vp = taskPool.gameObject.GetComponent<VideoPlayer>();
                vp.url = filefullpath;
                taskPool.gameObject.GetComponent<RawImage>().texture = vp.targetTexture;
                vp.Play();
            }
        }
    }
}
