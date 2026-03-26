// SPDX-License-Identifier: MPL-2.0

using Vogen;

[assembly: VogenDefaults(
    conversions: Conversions.EfCoreValueConverter,
    customizations: Customizations.AddFactoryMethodForGuids,
    staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon
)]
