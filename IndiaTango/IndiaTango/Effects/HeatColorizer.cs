// -----------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by:
//        The WPF ShaderEffect Generator
//        http://wpfshadergenerator.codeplex.com
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace IndiaTango.Effects
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Media3D;
    
    
    /// <summary>Heat Colorizer</summary>
    public class HeatColorizer : System.Windows.Media.Effects.ShaderEffect
    {
        
        /// <summary>The implicit input sampler passed into the pixel shader by WPF.</summary>
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(HeatColorizer), 0, SamplingMode.Auto);
        /// <summary>The 1x256 color map gradient to use to colorize the incoming grayscale input sampler.</summary>
        public static readonly DependencyProperty PaletteProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Palette", typeof(HeatColorizer), 1, SamplingMode.Auto);
        
        public HeatColorizer()
        {
            PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri("/B3;component/Effects/HeatColorizer.ps", UriKind.Relative);
            this.PixelShader = pixelShader;
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(PaletteProperty);
            this.DdxUvDdyUvRegisterIndex = -1;
        }
        
        /// <summary>The implicit input sampler passed into the pixel shader by WPF.</summary>
        public virtual System.Windows.Media.Brush Input
        {
            get
            {
                return ((System.Windows.Media.Brush)(this.GetValue(InputProperty)));
            }
            set
            {
                this.SetValue(InputProperty, value);
            }
        }
        
        /// <summary>The 1x256 color map gradient to use to colorize the incoming grayscale input sampler.</summary>
        public virtual System.Windows.Media.Brush Palette
        {
            get
            {
                return ((System.Windows.Media.Brush)(this.GetValue(PaletteProperty)));
            }
            set
            {
                this.SetValue(PaletteProperty, value);
            }
        }
    }
}