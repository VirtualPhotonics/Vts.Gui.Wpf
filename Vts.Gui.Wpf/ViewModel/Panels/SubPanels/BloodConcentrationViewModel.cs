using System;
using Vts.SpectralMapping;

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model implementing Blood Concentration sub-panel functionality
/// </summary>
public class BloodConcentrationViewModel : BindableObject
{
    // Backing-fields for chromophores. For consistency, all other properties are
    // designed to set these values, and do not have a backing store of their own

    public BloodConcentrationViewModel()
        : this(
            new ChromophoreAbsorber(ChromophoreType.Hb, 10.0),
            new ChromophoreAbsorber(ChromophoreType.HbO2, 30.0))
    {
    }

    public BloodConcentrationViewModel(IChromophoreAbsorber hb, IChromophoreAbsorber hbO2)
    {
        HbO2 = hbO2;
        Hb = hb;
    }

    /// <summary>
    ///     ChromophoreAbsorber representing the concentration of oxy-hemoglobin (uM)
    /// </summary>
    public bool DisplayBloodVM
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(DisplayBloodVM));
        }
    } = true;

    public IChromophoreAbsorber HbO2
    {
        get;
        set
        {
            if (field != null) // unsubscribe any existing property changed event
            {
                // first define delegate, then unsubscribe
                void Func(object s, EventArgs a) => UpdateStO2AndTotalHb();
                field.PropertyChanged -= Func;
            }

            if (value != null)
            {
                field = value;
                OnPropertyChanged(nameof(HbO2));
                // subscribe to the new property changed event
                field.PropertyChanged += (s, a) => UpdateStO2AndTotalHb();
                // make sure that whatever's bound to StO2 and TotalHb will update
                UpdateStO2AndTotalHb();
            }
        }
    }

    /// <summary>
    ///     ChromophoreAbsorber representing the concentration of deoxy-hemoglobin (uM)
    /// </summary>
    public IChromophoreAbsorber Hb
    {
        get;
        set
        {
            if (field != null) // unsubscribe any existing property changed event
            {
                // first define delegate, then unsubscribe
                void Func(object s, EventArgs a) => UpdateStO2AndTotalHb();
                field.PropertyChanged -= Func;
            }

            if (value != null)
            {
                field = value;
                //_Hb.Concentration = FormatOutput(_Hb.Concentration);
                OnPropertyChanged(nameof(Hb));
                // subscribe to the new property changed event
                field.PropertyChanged += (s, a) => UpdateStO2AndTotalHb();
                // make sure that whatever's bound to StO2 and TotalHb will update
                UpdateStO2AndTotalHb();
            }
        }
    }

    /// <summary>
    ///     Property to specify tissue oxygen saturation (unitless)
    /// </summary>
    /// <remarks>
    ///     This is just a pass-through to Hb and HbO2, based on the existing TotalHb value
    /// </remarks>
    public double StO2
    {
        get => HbO2.Concentration / (Hb.Concentration + HbO2.Concentration);
        set
        {
            // calculate the new Hb and HbO2 values based on the existing TotalHb
            // storing them in temporary local fields (to break the circular reference)
            var hb = (1 - value) * TotalHb;
            var hbO2 = value * TotalHb;

            // after calculated, assign them to the concentration properties of 
            // the ChromphoreAbsorber instances
            Hb.Concentration = hb;
            HbO2.Concentration = hbO2;

            OnPropertyChanged(nameof(StO2));
        }
    }

    /// <summary>
    ///     Property to specify total hemoglobin concentration, HbT (uM)
    /// </summary>
    /// <remarks>
    ///     This is just a pass-through to Hb and HbO2, based on the existing StO2 value
    /// </remarks>
    public double TotalHb
    {
        get => Hb.Concentration + HbO2.Concentration;
        set
        {
            // calculate the new Hb and HbO2 values based on the existing StO2
            // storing them in temporary local fields (to break the circular reference)
            var hbO2 = value * StO2;
            var hb = value * (1 - StO2);

            // after calculated, assign them to the concentration properties of 
            // the ChromphoreAbsorber instances
            Hb.Concentration = hb;
            HbO2.Concentration = hbO2;

            OnPropertyChanged(nameof(TotalHb));

            // call this last, because BloodVolumeFraction depends on TotalHb being updated
            OnPropertyChanged(nameof(BloodVolumeFraction));
        }
    }

    /// <summary>
    ///     Property to specify blood volume fraction (vb) as an alternative to TotalHb
    /// </summary>
    /// <remarks>
    ///     This is just a pass-through to TotalHb, assuming 150gHb/L for whole blood
    ///     todo: verify that the 150g/L value is for *whole blood* not RBCs
    ///     (otherwise, need to account for Hct)
    /// </remarks>
    public double BloodVolumeFraction
    {
        get => TotalHb / 1E6 * 64500 / 150;
        set
        {
            //BloodVolumeFraction = value * 1E6 / 64500 * 150;
            TotalHb = value * 1E6 / 64500 * 150; // TotalHb will internally fire OnPropertyChanged() here

            OnPropertyChanged(nameof(BloodVolumeFraction));
            OnPropertyChanged(nameof(TotalHb));
        }
    }

    private void UpdateStO2AndTotalHb()
    {
        OnPropertyChanged(nameof(StO2));
        OnPropertyChanged(nameof(TotalHb));
        OnPropertyChanged(nameof(BloodVolumeFraction));
    }
}