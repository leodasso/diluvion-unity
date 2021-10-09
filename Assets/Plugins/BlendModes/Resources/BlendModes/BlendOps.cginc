#ifndef BLEND_OPS
#define BLEND_OPS

#ifndef BLEND_RESULT_DECLARED
fixed4 blendResult;
#endif

#ifdef BmDarken
blendResult = Darken(grabColor, texColor);
#endif
#ifdef BmMultiply
blendResult = Multiply(grabColor, texColor);
#endif
#ifdef BmColorBurn
blendResult = ColorBurn(grabColor, texColor);
#endif
#ifdef BmLinearBurn
blendResult = LinearBurn(grabColor, texColor);
#endif
#ifdef BmDarkerColor
blendResult = DarkerColor(grabColor, texColor);
#endif
#ifdef BmLighten
blendResult = Lighten(grabColor, texColor);
#endif
#ifdef BmScreen
blendResult = Screen(grabColor, texColor);
#endif
#ifdef BmColorDodge
blendResult = ColorDodge(grabColor, texColor);
#endif
#ifdef BmLinearDodge
blendResult = LinearDodge(grabColor, texColor);
#endif
#ifdef BmLighterColor
blendResult = LighterColor(grabColor, texColor);
#endif
#ifdef BmOverlay
blendResult = Overlay(grabColor, texColor);
#endif
#ifdef BmSoftLight
blendResult = SoftLight(grabColor, texColor);
#endif
#ifdef BmHardLight
blendResult = HardLight(grabColor, texColor);
#endif
#ifdef BmVividLight
blendResult = VividLight(grabColor, texColor);
#endif
#ifdef BmLinearLight
blendResult = LinearLight(grabColor, texColor);
#endif
#ifdef BmPinLight
blendResult = PinLight(grabColor, texColor);
#endif
#ifdef BmHardMix
blendResult = HardMix(grabColor, texColor);
#endif
#ifdef BmDifference
blendResult = Difference(grabColor, texColor);
#endif
#ifdef BmExclusion
blendResult = Exclusion(grabColor, texColor);
#endif
#ifdef BmSubtract
blendResult = Subtract(grabColor, texColor);
#endif
#ifdef BmDivide
blendResult = Divide(grabColor, texColor);
#endif
#ifdef BmHue
blendResult = Hue(grabColor, texColor);
#endif
#ifdef BmSaturation
blendResult = Saturation(grabColor, texColor);
#endif
#ifdef BmColor
blendResult = Color(grabColor, texColor);
#endif
#ifdef BmLuminosity
blendResult = Luminosity(grabColor, texColor);
#endif
 
#endif
