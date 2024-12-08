using System.Windows.Controls;
using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public partial class CustomLine
    {
        protected override void InitControlPanel()
        {
            base.InitControlPanel();

            var addPointbutton =
                WpfHelper.CreateButton(
                    () =>
                    {
                        AddPoint(GetAnchorDefaultPosition());
                    },
                    "Add point"
                );

            _controls.Add("Add Point", addPointbutton);

            var removePointbutton =
                WpfHelper.CreateButton(
                    () =>
                    {
                        //RemovePoint();
                        RenderControlPanel();
                    },
                    "Remove point"
                );

            _controls.Add("Remove Point", removePointbutton);

            var isClosedCheckBox =
                WpfHelper.CreateLabeledCheckBox(
                    "Is Closed:",
                    this,
                    nameof(IsClosed)
                );

            _controls.Add("IsClosed", isClosedCheckBox);
        }
    }
}
