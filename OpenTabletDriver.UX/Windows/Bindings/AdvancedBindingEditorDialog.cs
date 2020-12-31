using System;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Reflection;
using OpenTabletDriver.UX.Controls.Generic;
using IBinding = OpenTabletDriver.Plugin.IBinding;

namespace OpenTabletDriver.UX.Windows.Bindings
{
    public class AdvancedBindingEditorDialog : Dialog<PluginSettingStore>
    {
        public AdvancedBindingEditorDialog(PluginSettingStore currentBinding = null)
        {
            Title = "Advanced Binding Editor";
            Result = currentBinding;
            Padding = 5;

            bindingPropertyGroup = new Group
            {
                Text = "Property"
            };

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Width = 300,
                Height = 250,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new StackLayout
                        {
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            Items =
                            {
                                new Group
                                {
                                    Text = "Type",
                                    Content = bindingTypeDropDown = new TypeDropDown<IBinding>() 
                                },
                                bindingPropertyGroup
                            }
                        }
                    },
                    new StackLayoutItem
                    {
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new Button(ClearBinding)
                                    {
                                        Text = "Clear"
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new Button(ApplyBinding)
                                    {
                                        Text = "Apply"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            bindingTypeDropDown.SelectedValueChanged += (sender, e) => bindingPropertyGroup.Content = GetPropertyControl();
            bindingTypeDropDown.SelectedType = currentBinding.GetPluginReference().GetTypeReference();

            string bindingProperty = currentBinding?["Property"]?.GetValue<string>();
            switch (bindingPropertyGroup.Content)
            {
                case BindingPropertyDropDown bindingPropertyDropDown:
                {
                    bindingPropertyDropDown.SelectedKey = bindingProperty;
                    break;
                }
                case TextControl textControl:
                {
                    textControl.Text = bindingProperty;
                    break;
                }
                default:
                {
                    throw new Exception("Invalid binding property group control");
                }
            };
        }

        public Control GetPropertyControl()
        {
            var pluginRef = AppInfo.PluginManager.GetPluginReference(bindingTypeDropDown.SelectedType);
            var type = pluginRef.GetTypeReference<IBinding>();
            bool isValidateBinding = typeof(IValidateBinding).IsAssignableFrom(type);

            return isValidateBinding ? new BindingPropertyDropDown(pluginRef.Construct<IValidateBinding>()) : new TextBox();
        }

        private TypeDropDown<IBinding> bindingTypeDropDown;
        private Group bindingPropertyGroup;

        private void ClearBinding(object sender, EventArgs e)
        {
            Close(null);
        }

        private void ApplyBinding(object sender, EventArgs e)
        {
            var pluginRef = AppInfo.PluginManager.GetPluginReference(bindingTypeDropDown.SelectedType);

            var binding = pluginRef.Construct<IBinding>();
            binding.Property = bindingPropertyGroup.Content switch
            {
                BindingPropertyDropDown propertyDropDown => propertyDropDown.SelectedKey,
                TextControl textControl => textControl.Text,
                _ => null
            };

            Close(new PluginSettingStore(binding));
        }

        private class BindingPropertyDropDown : DropDown
        {
            public BindingPropertyDropDown()
            {
            }

            public BindingPropertyDropDown(IValidateBinding validateBinding)
            {
                this.DataStore = validateBinding.ValidProperties;
            }
        }
    }
}
