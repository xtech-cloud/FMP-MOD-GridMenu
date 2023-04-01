
using System.Xml.Serialization;

namespace XTC.FMP.MOD.GridMenu.LIB.Unity
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class MyConfig : MyConfigBase
    {
        public class CellContent
        {
            [XmlAttribute("type")]
            public string type { get; set; } = "";
            [XmlArray("ParameterS"), XmlArrayItem("Parameter")]
            public Parameter[] parameterS { get; set; } = new Parameter[0];
            [XmlArray("SubjectS"), XmlArrayItem("Subject")]
            public Subject[] subjectS { get; set; } = new Subject[0];
        }

        public class Background
        {
            [XmlAttribute("image")]
            public string image { get; set; } = "";
        }


        public class Debug
        {
            [XmlAttribute("active")]
            public bool active { get; set; } = false;
            [XmlAttribute("lineColor")]
            public string lineColor { get; set; } = "#FF0000FF";
            [XmlAttribute("cellColor")]
            public string cellColor { get; set; } = "#00FF0088";
            [XmlAttribute("drawLine")]
            public bool drawLine { get; set; } = true;
            [XmlAttribute("drawCell")]
            public bool drawCell { get; set; } = true;
        }

        public class Cell
        {
            [XmlAttribute("name")]
            public string name { get; set; } = "";
            [XmlAttribute("columnStart")]
            public int columnStart { get; set; } = 0;
            [XmlAttribute("columnEnd")]
            public int columnEnd { get; set; } = 0;
            [XmlAttribute("rowStart")]
            public int rowStart { get; set; } = 0;
            [XmlAttribute("rowEnd")]
            public int rowEnd { get; set; } = 0;
            [XmlElement("Content")]
            public CellContent content { get; set; } = new CellContent();
        }

        public class Grid
        {
            [XmlAttribute("column")]
            public int column { get; set; } = 48;
            [XmlAttribute("row")]
            public int row { get; set; } = 27;
        }

        public class Style
        {
            [XmlAttribute("name")]
            public string name { get; set; } = "";
            [XmlElement("Background")]
            public Background background { get; set; } = new Background();
            [XmlElement("Grid")]
            public Grid grid { get; set; } = new Grid();
            [XmlArray("CellS"), XmlArrayItem("Cell")]
            public Cell[] cellS { get; set; } = new Cell[0];
            [XmlElement("Debug")]
            public Debug debug { get; set; } = new Debug();
        }


        [XmlArray("Styles"), XmlArrayItem("Style")]
        public Style[] styles { get; set; } = new Style[0];
    }
}

