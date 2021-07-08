// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using FancyZonesEditor.Models;

namespace FancyZonesEditor
{
    public partial class CanvasEditorWindow : EditorWindow
    {
        private CanvasLayoutModel _model;
        private CanvasLayoutModel _stashedModel;

        public CanvasEditorWindow()
            : base()
        {
            InitializeComponent();

            KeyUp += CanvasEditorWindow_KeyUp;
            KeyDown += CanvasEditorWindow_KeyDown;

            _model = App.Overlay.CurrentDataContext as CanvasLayoutModel;
            _stashedModel = (CanvasLayoutModel)_model.Clone();
        }

        public LayoutModel Model
        {
            get
            {
                return _model;
            }
        }

        private void OnAddZone(object sender, RoutedEventArgs e)
        {
            this.Focus();
            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                CanvasEditorWindowAutomationPeer peer =
                    UIElementAutomationPeer.FromElement(this) as CanvasEditorWindowAutomationPeer;

                if (peer != null)
                {
                    peer.RaisePropertyChangedEvent(
                        ValuePatternIdentifiers.ValueProperty,
                        "AAAAAAAAAAAA",
                        "BBBBBBBBBBBBBB");
                }
            }

            this.Value += "This is new value Stefan";
            _model.AddZone();
        }

        protected new void OnCancel(object sender, RoutedEventArgs e)
        {
            base.OnCancel(sender, e);
            _stashedModel.RestoreTo(_model);
        }

        private void CanvasEditorWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                OnCancel(sender, null);
            }
        }

        private void CanvasEditorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                e.Handled = true;
                App.Overlay.FocusEditor();
            }
        }

        // Custom automation peer to fire event on zone add
        private const string DefaultValue = "default";

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            "Value", typeof(string), typeof(CanvasEditorWindow), new FrameworkPropertyMetadata(DefaultValue, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(CoerceValue)));

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            CanvasEditorWindow nudCtrl = (CanvasEditorWindow)obj;
            nudCtrl.Focus();
            string oldValue = (string)args.OldValue;
            string newValue = (string)args.NewValue + "ADASDA";

            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                CanvasEditorWindowAutomationPeer peer =
                    UIElementAutomationPeer.FromElement(nudCtrl) as CanvasEditorWindowAutomationPeer;

                if (peer != null)
                {
                    peer.RaisePropertyChangedEvent(
                        ValuePatternIdentifiers.ValueProperty,
                        (string)oldValue,
                        (string)newValue);
                }
            }

            RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
                oldValue, newValue, ValueChangedEvent);

            nudCtrl.OnValueChanged(e);
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
    "ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(CanvasEditorWindow));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<string> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceValue(DependencyObject element, object value)
        {
            string newValue = (string)value;
            return newValue;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CanvasEditorWindowAutomationPeer(this);
        }

        public class CanvasEditorWindowAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
        {
            public CanvasEditorWindowAutomationPeer(CanvasEditorWindow control)
                : base(control)
            {
            }

            protected override string GetClassNameCore()
            {
                return "CanvasEditorWindow";
            }

            protected override AutomationControlType GetAutomationControlTypeCore()
            {
                return AutomationControlType.Window;
            }

            public override object GetPattern(PatternInterface patternInterface)
            {
                if (patternInterface == PatternInterface.Value)
                {
                    return this;
                }

                return base.GetPattern(patternInterface);
            }

            public void SetValue(string value)
            {
                MyOwner.Value = value + "AA";
            }

            private CanvasEditorWindow MyOwner
            {
                get
                {
                    return (CanvasEditorWindow)Owner;
                }
            }

            public string Value
            {
                get { return MyOwner.Value; }
            }

            public bool IsReadOnly
            {
                get { return !IsEnabled(); }
            }
        }
    }
}
