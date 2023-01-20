using System;
using Xamarin.Forms;

namespace BottomDrawer
{
    public class BottomDrawer : Behavior<View>
    {
        #region "Atributos"
        private double translationYStart;
        private View attachedView;
        #endregion

        #region "Propiedades"
        public uint AnimationDuration
        {
            get => (uint)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(nameof(AnimationDuration), typeof(uint), typeof(BottomDrawer), Convert.ToUInt32(250), BindingMode.OneWay);

        public bool IsDragging
        {
            get => (bool)GetValue(IsDraggingProperty);
            set => SetValue(IsDraggingProperty, value);
        }

        public static readonly BindableProperty IsDraggingProperty = BindableProperty.Create(nameof(IsDragging), typeof(bool), typeof(BottomDrawer), false, BindingMode.OneWay);

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(BottomDrawer), false, BindingMode.TwoWay, propertyChanged: IsExpandedPropertyChanged);

        public double DragPercent
        {
            get => (double)GetValue(DragPercentProperty);
            set => SetValue(DragPercentProperty, value);
        }

        public static readonly BindableProperty DragPercentProperty = BindableProperty.Create(nameof(DragPercent), typeof(double), typeof(BottomDrawer), default(double), BindingMode.TwoWay, propertyChanged: DragPercentChanged);

        public double[] LockStates
        {
            get => (double[])GetValue(LockStatesProperty);
            set => SetValue(LockStatesProperty, value);
        }

        public static readonly BindableProperty LockStatesProperty = BindableProperty.Create(nameof(LockStates), typeof(double[]), typeof(BottomDrawer), new double[] { 0, .4, .75 });
        #endregion

        #region "Eventos"
        private static void DragPercentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is double expandValue && bindable is BottomDrawer drawer)
            {
                if (!drawer.IsDragging)
                {
                    double proportionY = drawer.GetProportionCoordinate(expandValue);
                    double finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(proportionY));
                    if (expandValue < 0)
                    {
                        drawer.attachedView.TranslateTo(x: drawer.attachedView.X, y: finalTranslation, length: 250, easing: Easing.SpringIn);
                    }
                    else
                    {
                        drawer.attachedView.TranslateTo(x: drawer.attachedView.X, y: finalTranslation, length: 250, easing: Easing.SpringOut);
                    }
                }
            }
        }

        private static void IsExpandedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is bool isExpanded && bindable is BottomDrawer drawer)
            {
                if (!drawer.IsDragging)
                {
                    if (!isExpanded)
                    {
                        drawer.Dismiss();
                    }
                    else
                    {
                        drawer.Open();
                    }
                }
            }
        }

        private void OnPanChanged(object sender, PanUpdatedEventArgs e)
        {
            double Y =
                (Device.RuntimePlatform == Device.Android
                    ? this.attachedView.TranslationY
                    : this.translationYStart)
                + e.TotalY;
            double tmpExpandedPercentage = GetPropertionDistance(Y);


            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    IsDragging = true;
                    double translateY = Math.Max(Math.Min(0, Y), -Math.Abs((this.attachedView.Height * .25) - this.attachedView.Height));
                    this.attachedView.TranslateTo(x: this.attachedView.X, y: translateY, length: 1);
                    this.DragPercent = tmpExpandedPercentage;
                    break;

                case GestureStatus.Completed:
                    double dragDistanceY1 = e.TotalY + this.attachedView.TranslationY;
                    double tmpLockState = GetClosestLockStatePercentage(this.DragPercent);
                    double tmpLockStateY = GetProportionCoordinate(tmpLockState);
                    double finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(tmpLockStateY));
                    if (DetectSwipeUp(e))
                    {
                        this.attachedView.TranslateTo(x: this.attachedView.X, y: finalTranslation, length: AnimationDuration, easing: Easing.SpringIn);
                    }
                    else
                    {
                        this.attachedView.TranslateTo(x: this.attachedView.X, y: finalTranslation, length: AnimationDuration, easing: Easing.SpringOut);
                    }

                    this.DragPercent = tmpLockState;
                    this.IsDragging = false;
                    break;

                case GestureStatus.Started:
                    this.translationYStart = this.attachedView.TranslationY;
                    break;
            }

            if (LockStates.Length <= 0)
            {
                return;
            }

            int indexOfLastLockState = LockStates.Length - 1;
            double lastLockState = LockStates[indexOfLastLockState];
            double expandedPercentageBeforeLock = DragPercent;
            if (DragPercent > lastLockState)
            {
                DragPercent = lastLockState;
            }

            IsExpanded = (DragPercent > 0);
        }

        private void OnTapped(object sender, EventArgs e)
        {
            if (!this.IsExpanded)
            {
                if (this.LockStates.Length >= 2)
                {
                    this.DragPercent = LockStates[1];
                }
                this.IsExpanded = (this.DragPercent > 0);
            }
        }
        #endregion

        #region "Métodos"
        private bool DetectSwipeUp(PanUpdatedEventArgs e)
        {
            return e.TotalY < 0;
        }

        public void Dismiss()
        {
            double finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(GetProportionCoordinate(LockStates[0])));
            this.attachedView.TranslateTo(x: this.attachedView.X, y: finalTranslation, length: 400, easing: Device.RuntimePlatform == Device.Android ? Easing.SpringOut : null);
        }

        private double GetClosestLockStatePercentage(double currentPercentageVisible)
        {
            double current = currentPercentageVisible;
            var smallestDistance = 10000.0;
            var closestIndex = 0;

            for (int i = 0; i < LockStates.Length; i++)
            {
                var state = LockStates[i];
                var absoluteDistance = Math.Abs(state - current);
                if (absoluteDistance < smallestDistance)
                {
                    smallestDistance = absoluteDistance;
                    closestIndex = i;
                }
            }

            double result = LockStates[closestIndex];
            return result;
        }

        private double GetProportionCoordinate(double proportion)
        {
            return proportion * attachedView.Height;
        }

        private double GetPropertionDistance(double TranslationY)
        {
            return Math.Abs(TranslationY) / this.attachedView.Height;
        }

        private void Open()
        {
            double finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(GetProportionCoordinate(LockStates[LockStates.Length - 1])));
            this.attachedView.TranslateTo(x: this.attachedView.X, y: finalTranslation, length: 150, easing: Device.RuntimePlatform == Device.Android ? Easing.SpringIn : null);
        }
        #endregion

        #region "Protected"
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            BindingContext = attachedView.BindingContext;
        }

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);
            this.attachedView = bindable;

            PanGestureRecognizer panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanChanged;
            this.attachedView.GestureRecognizers.Add(panGesture);

            TapGestureRecognizer tapGestura = new TapGestureRecognizer();
            tapGestura.Tapped += OnTapped;
            this.attachedView.GestureRecognizers.Add(tapGestura);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);
        }
        #endregion
    }
}
