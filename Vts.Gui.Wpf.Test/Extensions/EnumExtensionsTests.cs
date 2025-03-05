using System;
using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Test.Extensions
{
    /// <summary>
    /// Tests EnumExtensions classes
    /// </summary>
    [TestFixture]
    public class EnumExtensionsTests
    {
        /// <summary>
        /// Verifies extension method IsGaussianForwardModel returns correct value
        /// </summary>
        [Test]
        public void Verify_method_IsGaussianForwardModel_returns_correct_value()
        {
            Assert.That(ForwardSolverType.PointSourceSDA.IsGaussianForwardModel(), Is.False);
            Assert.That(ForwardSolverType.DeltaPOne.IsGaussianForwardModel(), Is.False);
            Assert.That(ForwardSolverType.MonteCarlo.IsGaussianForwardModel(), Is.False);
            Assert.That(ForwardSolverType.TwoLayerSDA.IsGaussianForwardModel(), Is.False);
            Assert.That(ForwardSolverType.DistributedGaussianSourceSDA.IsGaussianForwardModel(), Is.True);
        }

        /// <summary>
        /// Verifies extension method IsMultiRegionForwardModel returns correct value
        /// </summary>
        [Test]
        public void Verify_method_IsMultiRegionForwardModel_returns_correct_value()
        {
            Assert.That(ForwardSolverType.PointSourceSDA.IsMultiRegionForwardModel(), Is.False);
            Assert.That(ForwardSolverType.DeltaPOne.IsMultiRegionForwardModel(), Is.False);
            Assert.That(ForwardSolverType.MonteCarlo.IsMultiRegionForwardModel(), Is.False);
            Assert.That(ForwardSolverType.DistributedGaussianSourceSDA.IsMultiRegionForwardModel(), Is.False);
            Assert.That(ForwardSolverType.TwoLayerSDA.IsMultiRegionForwardModel(), Is.True);
        }

        /// <summary>
        /// Verifies extension method GetMaxArgumentLocation returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetMaxArgumentLocation_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Time.GetMaxArgumentLocation(), Is.EqualTo(2));
            Assert.That(IndependentVariableAxis.Rho.GetMaxArgumentLocation(), Is.EqualTo(0));
            Assert.That(IndependentVariableAxis.Fx.GetMaxArgumentLocation(), Is.EqualTo(0));
            Assert.That(IndependentVariableAxis.Ft.GetMaxArgumentLocation(), Is.EqualTo(2));
            Assert.That(IndependentVariableAxis.Z.GetMaxArgumentLocation(), Is.EqualTo(1));
            Assert.That(IndependentVariableAxis.Wavelength.GetMaxArgumentLocation(), Is.EqualTo(2));
        }

        /// <summary>
        /// Verified that extension method throws an exception
        /// </summary>
        [Test]
        public void Verify_GetMaxArgumentLocation_throws_exception()
        {
            const IndependentVariableAxis invalidEnum = (IndependentVariableAxis)99;
            Assert.Throws<NotImplementedException>(() => invalidEnum.GetMaxArgumentLocation());
        }

        /// <summary>
        /// Verify that extension method returns the correct value
        /// </summary>
        [Test]
        public void Verify_IsSpatialAxis_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Rho.IsSpatialAxis(), Is.True);
            Assert.That(IndependentVariableAxis.Fx.IsSpatialAxis(), Is.True);
            Assert.That(IndependentVariableAxis.Time.IsSpatialAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Ft.IsSpatialAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Z.IsSpatialAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Wavelength.IsSpatialAxis(), Is.False);
        }

        /// <summary>
        /// Verified that extension method throws an exception
        /// </summary>
        [Test]
        public void Verify_IsSpatialAxis_throws_exception()
        {
            const IndependentVariableAxis invalidEnum = (IndependentVariableAxis)99;
            Assert.Throws<NotImplementedException>(() => invalidEnum.IsSpatialAxis());
        }

        /// <summary>
        /// Verify that extension method returns the correct value
        /// </summary>
        [Test]
        public void Verify_IsTemporalAxis_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Time.IsTemporalAxis(), Is.True);
            Assert.That(IndependentVariableAxis.Ft.IsTemporalAxis(), Is.True);
            Assert.That(IndependentVariableAxis.Rho.IsTemporalAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Fx.IsTemporalAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Z.IsTemporalAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Wavelength.IsTemporalAxis(), Is.False);
        }

        /// <summary>
        /// Verified that extension method throws an exception
        /// </summary>
        [Test]
        public void Verify_IsTemporalAxis_throws_exception()
        {
            const IndependentVariableAxis invalidEnum = (IndependentVariableAxis)99;
            Assert.Throws<NotImplementedException>(() => invalidEnum.IsTemporalAxis());
        }

        /// <summary>
        /// Verify that extension method returns the correct value
        /// </summary>
        [Test]
        public void Verify_IsDepthAxis_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Z.IsDepthAxis(), Is.True);
            Assert.That(IndependentVariableAxis.Time.IsDepthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Ft.IsDepthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Rho.IsDepthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Fx.IsDepthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Wavelength.IsDepthAxis(), Is.False);
        }

        /// <summary>
        /// Verified that extension method throws an exception
        /// </summary>
        [Test]
        public void Verify_IsDepthAxis_throws_exception()
        {
            const IndependentVariableAxis invalidEnum = (IndependentVariableAxis)99;
            Assert.Throws<NotImplementedException>(() => invalidEnum.IsDepthAxis());
        }

        /// <summary>
        /// Verify that extension method returns the correct value
        /// </summary>
        [Test]
        public void Verify_IsWavelengthAxis_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Wavelength.IsWavelengthAxis(), Is.True);
            Assert.That(IndependentVariableAxis.Z.IsWavelengthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Time.IsWavelengthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Ft.IsWavelengthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Rho.IsWavelengthAxis(), Is.False);
            Assert.That(IndependentVariableAxis.Fx.IsWavelengthAxis(), Is.False);
        }

        /// <summary>
        /// Verified that extension method throws an exception
        /// </summary>
        [Test]
        public void Verify_IsWavelengthAxis_throws_exception()
        {
            const IndependentVariableAxis invalidEnum = (IndependentVariableAxis)99;
            Assert.Throws<NotImplementedException>(() => invalidEnum.IsWavelengthAxis());
        }

        /// <summary>
        /// Verifies extension method GetUnits for IndependentVariableAxis returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetUnits_for_IndependentVariableAxis_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Time.GetUnits(), Is.EqualTo(IndependentVariableAxisUnits.NS.GetInternationalizedString()));
            Assert.That(IndependentVariableAxis.Rho.GetUnits(), Is.EqualTo(IndependentVariableAxisUnits.MM.GetInternationalizedString()));
            Assert.That(IndependentVariableAxis.Fx.GetUnits(), Is.EqualTo(IndependentVariableAxisUnits.InverseMM.GetInternationalizedString()));
            Assert.That(IndependentVariableAxis.Ft.GetUnits(), Is.EqualTo(IndependentVariableAxisUnits.GHz.GetInternationalizedString()));
            Assert.That(IndependentVariableAxis.Wavelength.GetUnits(), Is.EqualTo(IndependentVariableAxisUnits.NM.GetInternationalizedString()));
        }

        /// <summary>
        /// Verifies extension method GetTitle for IndependentVariableAxis returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetTitle_for_IndependentVariableAxis_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Rho.GetTitle(), Is.EqualTo(IndependentVariableAxis.Rho.GetLocalizedString()));
            Assert.That(IndependentVariableAxis.Time.GetTitle(), Is.EqualTo(IndependentVariableAxis.Time.GetLocalizedString()));
            Assert.That(IndependentVariableAxis.Fx.GetTitle(), Is.EqualTo(IndependentVariableAxis.Fx.GetLocalizedString()));
            Assert.That(IndependentVariableAxis.Ft.GetTitle(), Is.EqualTo(IndependentVariableAxis.Ft.GetLocalizedString()));
            Assert.That(IndependentVariableAxis.Wavelength.GetTitle(), Is.EqualTo(IndependentVariableAxis.Wavelength.GetLocalizedString()));
        }

        /// <summary>
        /// Verifies extension method GetUnits for SolutionDomainType returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetUnits_for_SolutionDomainType_returns_correct_value()
        {
            Assert.That(SolutionDomainType.ROfRho.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMSquared.GetInternationalizedString()));
            Assert.That(SolutionDomainType.ROfFx.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.Unitless.GetInternationalizedString()));
            Assert.That(SolutionDomainType.ROfRhoAndTime.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMSquaredPerNS.GetInternationalizedString()));
            Assert.That(SolutionDomainType.ROfFxAndTime.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerNS.GetInternationalizedString()));
            Assert.That(SolutionDomainType.ROfRhoAndFt.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMSquaredPerGHz.GetInternationalizedString()));
            Assert.That(SolutionDomainType.ROfFxAndFt.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerGHz.GetInternationalizedString()));
        }

        /// <summary>
        /// Verifies extension method GetUnits for FluenceSolutionDomainType returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetUnits_for_FluenceSolutionDomainType_returns_correct_value()
        {
            Assert.That(FluenceSolutionDomainType.FluenceOfRhoAndZ.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMCubed.GetInternationalizedString()));
            Assert.That(FluenceSolutionDomainType.FluenceOfFxAndZ.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMM.GetInternationalizedString()));
            Assert.That(FluenceSolutionDomainType.FluenceOfRhoAndZAndTime.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMCubedPerNS.GetInternationalizedString()));
            Assert.That(FluenceSolutionDomainType.FluenceOfFxAndZAndTime.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMPerNS.GetInternationalizedString()));
            Assert.That(FluenceSolutionDomainType.FluenceOfRhoAndZAndFt.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMCubedPerGHz.GetInternationalizedString()));
            Assert.That(FluenceSolutionDomainType.FluenceOfFxAndZAndFt.GetUnits(), Is.EqualTo(DependentVariableAxisUnits.PerMMPerGHz.GetInternationalizedString()));
        }


        /// <summary>
        /// Verifies extension method GetDefaultRange returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetDefaultRange_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Time.GetDefaultRange().Start, Is.EqualTo(0));
            Assert.That(IndependentVariableAxis.Time.GetDefaultRange().Stop, Is.EqualTo(0.05));
            Assert.That(IndependentVariableAxis.Time.GetDefaultRange().Count, Is.EqualTo(51));
            Assert.That(IndependentVariableAxis.Rho.GetDefaultRange().Start, Is.EqualTo(0.5));
            Assert.That(IndependentVariableAxis.Rho.GetDefaultRange().Stop, Is.EqualTo(9.5));
            Assert.That(IndependentVariableAxis.Rho.GetDefaultRange().Count, Is.EqualTo(19));
            Assert.That(IndependentVariableAxis.Fx.GetDefaultRange().Start, Is.EqualTo(0));
            Assert.That(IndependentVariableAxis.Fx.GetDefaultRange().Stop, Is.EqualTo(0.5));
            Assert.That(IndependentVariableAxis.Fx.GetDefaultRange().Count, Is.EqualTo(51));
            Assert.That(IndependentVariableAxis.Ft.GetDefaultRange().Start, Is.EqualTo(0));
            Assert.That(IndependentVariableAxis.Ft.GetDefaultRange().Stop, Is.EqualTo(0.5));
            Assert.That(IndependentVariableAxis.Ft.GetDefaultRange().Count, Is.EqualTo(51));
            Assert.That(IndependentVariableAxis.Wavelength.GetDefaultRange().Start, Is.EqualTo(650));
            Assert.That(IndependentVariableAxis.Wavelength.GetDefaultRange().Stop, Is.EqualTo(1000));
            Assert.That(IndependentVariableAxis.Wavelength.GetDefaultRange().Count, Is.EqualTo(36));
        }


        /// <summary>
        /// Verifies extension method GetDefaultConstantAxisValue returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetDefaultConstantAxisValue_returns_correct_value()
        {
            Assert.That(IndependentVariableAxis.Time.GetDefaultConstantAxisValue(), Is.EqualTo(0.05));
            Assert.That(IndependentVariableAxis.Rho.GetDefaultConstantAxisValue(), Is.EqualTo(1));
            Assert.That(IndependentVariableAxis.Fx.GetDefaultConstantAxisValue(), Is.EqualTo(0.0));
            Assert.That(IndependentVariableAxis.Ft.GetDefaultConstantAxisValue(), Is.EqualTo(0.0));
            Assert.That(IndependentVariableAxis.Wavelength.GetDefaultConstantAxisValue(), Is.EqualTo(650.0));
        }
    }
}
