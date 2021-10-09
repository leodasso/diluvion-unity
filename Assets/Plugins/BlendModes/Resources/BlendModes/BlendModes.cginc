#ifndef BLEND_MODES
#define BLEND_MODES

fixed HueToRgb(fixed3 pqt)
{
    if (pqt.z < .0) pqt.z += 1.0;
    if (pqt.z > 1.0) pqt.z -= 1.0;
    if (pqt.z < 1.0 / 6.0) return pqt.x + (pqt.y - pqt.x) * 6.0 * pqt.z;
    if (pqt.z < 1.0 / 2.0) return pqt.y;
    if (pqt.z < 2.0 / 3.0) return pqt.x + (pqt.y - pqt.x) * (2.0 / 3.0 - pqt.z) * 6.0;

    return pqt.x;
}

fixed3 HslToRgb (fixed3 hsl)
{ 
	fixed3 rgb;
	fixed3 pqt;

    if (hsl.y == 0) rgb = hsl.z; 
    else
    {
        pqt.y = hsl.z < .5 ? hsl.z * (1.0 + hsl.y) : hsl.z + hsl.y - hsl.z * hsl.y;
        pqt.x = 2.0 * hsl.z - pqt.y;
        rgb.r = HueToRgb(fixed3(pqt.x, pqt.y, hsl.x + 1.0 / 3.0));
        rgb.g = HueToRgb(fixed3(pqt.x, pqt.y, hsl.x));
        rgb.b = HueToRgb(fixed3(pqt.x, pqt.y, hsl.x - 1.0 / 3.0));
    }

    return rgb;
}

fixed3 RgbToHsl(fixed3 rgb)
{
    fixed maxC = max(rgb.r, max(rgb.g, rgb.b));
    fixed minC = min(rgb.r, min(rgb.g, rgb.b));

    fixed3 hsl;

    hsl = (maxC + minC) / 2.0;

    if (maxC == minC) hsl.x = hsl.y = .0;
    else
    {
        fixed d = maxC - minC;
        hsl.y = (hsl.z > .5) ? d / (2.0 - maxC - minC) : d / (maxC + minC);

        if (rgb.r > rgb.g && rgb.r > rgb.b) 
        	hsl.x = (rgb.g - rgb.b) / d + (rgb.g < rgb.b ? 6.0 : .0);
        else if (rgb.g > rgb.b) 
        	hsl.x = (rgb.b - rgb.r) / d + 2.0;
        else 
        	hsl.x = (rgb.r - rgb.g) / d + 4.0;

        hsl.x /= 6.0f;
    }

    return hsl;
}

fixed G (fixed4 c) { return .299 * c.r + .587 * c.g + .114 * c.b; }
 
fixed4 Darken (fixed4 a, fixed4 b)
{ 
	fixed4 r = min(a, b);
	r.a = b.a;
	return r;
}

fixed4 Multiply (fixed4 a, fixed4 b)
{ 
	fixed4 r = a * b;
	r.a = b.a;
	return r;
}

fixed4 ColorBurn (fixed4 a, fixed4 b) 
{ 
	fixed4 r = 1.0 - (1.0 - a) / b;
	r.a = b.a;
	return r;
}

fixed4 LinearBurn (fixed4 a, fixed4 b)
{ 
	fixed4 r = a + b - 1.0;
	r.a = b.a;
	return r;
}

fixed4 DarkerColor (fixed4 a, fixed4 b) 
{ 
	fixed4 r = G(a) < G(b) ? a : b;
	r.a = b.a;
	return r; 
}

fixed4 Lighten (fixed4 a, fixed4 b)
{ 
	fixed4 r = max(a, b);
	r.a = b.a;
	return r;
}

fixed4 Screen (fixed4 a, fixed4 b) 
{ 	
	fixed4 r = 1.0 - (1.0 - a) * (1.0 - b);
	r.a = b.a;
	return r;
}

fixed4 ColorDodge (fixed4 a, fixed4 b) 
{ 
	fixed4 r = a / (1.0 - b);
	r.a = b.a;
	return r;
}

fixed4 LinearDodge (fixed4 a, fixed4 b)
{ 
	fixed4 r = a + b;
	r.a = b.a;
	return r;
} 

fixed4 LighterColor (fixed4 a, fixed4 b) 
{ 
	fixed4 r = G(a) > G(b) ? a : b;
	r.a = b.a;
	return r; 
}

fixed4 Overlay (fixed4 a, fixed4 b) 
{
	fixed4 r = a > .5 ? 1.0 - 2.0 * (1.0 - a) * (1.0 - b) : 2.0 * a * b;
	r.a = b.a;
	return r;
}

fixed4 SoftLight (fixed4 a, fixed4 b)
{
	fixed4 r = (1.0 - a) * a * b + a * (1.0 - (1.0 - a) * (1.0 - b));
	r.a = b.a;
	return r;
}

fixed4 HardLight (fixed4 a, fixed4 b)
{
	fixed4 r = b > .5 ? 1.0 - (1.0 - a) * (1.0 - 2.0 * (b - .5)) : a * (2.0 * b);
	r.a = b.a;
	return r;
}

fixed4 VividLight (fixed4 a, fixed4 b)
{
	fixed4 r = b > .5 ? a / (1.0 - (b - .5) * 2.0) : 1.0 - (1.0 - a) / (b * 2.0);
	r.a = b.a;
	return r;
}

fixed4 LinearLight (fixed4 a, fixed4 b)
{
	fixed4 r = b > .5 ? a + 2.0 * (b - .5) : a + 2.0 * b - 1.0;
	r.a = b.a;
	return r;
}

fixed4 PinLight (fixed4 a, fixed4 b)
{
	fixed4 r = b > .5 ? max(a, 2.0 * (b - .5)) : min(a, 2.0 * b);
	r.a = b.a;
	return r;
}

fixed4 HardMix (fixed4 a, fixed4 b)
{
	fixed4 r = (b > 1.0 - a) ? 1.0 : .0;
	r.a = b.a;
	return r;
}

fixed4 Difference (fixed4 a, fixed4 b) 
{ 
	fixed4 r = abs(a - b);
	r.a = b.a;
	return r; 
}

fixed4 Exclusion (fixed4 a, fixed4 b)
{ 
	fixed4 r = a + b - 2.0 * a * b;
	r.a = b.a;
	return r; 
}

fixed4 Subtract (fixed4 a, fixed4 b)
{ 
	fixed4 r = a - b;
	r.a = b.a;
	return r; 
}

fixed4 Divide (fixed4 a, fixed4 b)
{ 
	fixed4 r = a / b;
	r.a = b.a;
	return r; 
}

fixed4 Hue (fixed4 a, fixed4 b)
{ 
	fixed3 aHsl = RgbToHsl(a.rgb);
	fixed3 bHsl = RgbToHsl(b.rgb);
	fixed3 rHsl = fixed3(bHsl.x, aHsl.y, aHsl.z);

	return fixed4(HslToRgb(rHsl), b.a);
}

fixed4 Saturation (fixed4 a, fixed4 b)
{ 
	fixed3 aHsl = RgbToHsl(a.rgb);
	fixed3 bHsl = RgbToHsl(b.rgb);
	fixed3 rHsl = fixed3(aHsl.x, bHsl.y, aHsl.z);

	return fixed4(HslToRgb(rHsl), b.a);
}

fixed4 Color (fixed4 a, fixed4 b)
{ 
	fixed3 aHsl = RgbToHsl(a.rgb);
	fixed3 bHsl = RgbToHsl(b.rgb);
	fixed3 rHsl = fixed3(bHsl.x, bHsl.y, aHsl.z);

	return fixed4(HslToRgb(rHsl), b.a);
}

fixed4 Luminosity (fixed4 a, fixed4 b)
{ 
	fixed3 aHsl = RgbToHsl(a.rgb);
	fixed3 bHsl = RgbToHsl(b.rgb);
	fixed3 rHsl = fixed3(aHsl.x, aHsl.y, bHsl.z);

	return fixed4(HslToRgb(rHsl), b.a);
}
 
#endif
