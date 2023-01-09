using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace DrawerDemo.Behavior
{
    internal class SwipeDrawerBehavior : Behavior<View>
    {

        public double LimitAction
        {
            get => (double)GetValue(LimitActionProperty);
            set => SetValue(LimitActionProperty, value);
        }
        public static readonly BindableProperty LimitActionProperty = BindableProperty.Create(nameof(LimitAction), typeof(double), typeof(SwipeDrawerBehavior), 30,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var me = (SwipeDrawerBehavior)bindable;
            me.LimitAction = (double)newValue;
        });

        private View view;
        private double lastPanY;

        readonly PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();

        protected override void OnAttachedTo(View bindable)
        {
            bindable.GestureRecognizers.Add(panGestureRecognizer);
            view = bindable;
            panGestureRecognizer.PanUpdated += OnPanUpdated;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            panGestureRecognizer.PanUpdated -= OnPanUpdated;
            bindable.GestureRecognizers.Remove(panGestureRecognizer);
            base.OnDetachingFrom(bindable);
        }

        private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    break;
                case GestureStatus.Running:
                    lastPanY = e.TotalY;
                    if (e.TotalY > 0)
                    {
                        await view.TranslateTo(0, e.TotalY);
                        if (e.TotalY > LimitAction) CloseDrawer?.Invoke(this, new EventArgs());
                    }
                    break;
                case GestureStatus.Completed:
                    if (lastPanY < LimitAction) OpenDrawer?.Invoke(this, new EventArgs());
                    else CloseDrawer?.Invoke(this, new EventArgs());
                    break;
                case GestureStatus.Canceled:
                    break;
                default:
                    break;
            }
            Swiped?.Invoke(this, e);
        }

        public event EventHandler<EventArgs> OpenDrawer;
        public event EventHandler<EventArgs> CloseDrawer;
        public event EventHandler<PanUpdatedEventArgs> Swiped;
    }
}
