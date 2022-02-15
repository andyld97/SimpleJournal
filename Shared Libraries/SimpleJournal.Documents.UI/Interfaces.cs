using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleJournal.Documents.UI
{
    public interface IPolygonTemplateInfo
    {
        Color BorderColor { get; set; }

        Color BackgroundColor { get; set; }
    }
}
