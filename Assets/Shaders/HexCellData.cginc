sampler2D _HexCellData;
float4 _HexCellData_TexelSize;

float _Hex_InnerRadiusTimesTwo;
float _Hex_OuterRadiusTimesThree;

float4 GetCellDataFromWorld(float3 worldPosition) {
	float x = worldPosition.x / _Hex_InnerRadiusTimesTwo;
	float y = -x;

	float offset = worldPosition.z / _Hex_OuterRadiusTimesThree;

	x -= offset;
	y -= offset;

	int roundedX = round(x);
	int roundedY = round(y);
	int roundedZ = round(-x - y);

	if (roundedX + roundedY + roundedZ != 0) {
		float deltaX = abs(x - roundedX);
		float deltaY = abs(y - roundedY);
		float deltaZ = abs(-x - y - roundedZ);

		if (deltaX > deltaY && deltaX > deltaZ) {
			roundedX = -roundedY - roundedZ;

		}else if (deltaZ > deltaY) {
			roundedZ = -roundedX - roundedY;
		}
	}

	int xoffset = roundedX + (roundedZ - (roundedZ & 1)) / 2;
	int zOffset = roundedZ;

	float2 uv = float2((xoffset + 0.5) * _HexCellData_TexelSize.x, (zOffset + 0.5) * _HexCellData_TexelSize.y);

	float4 data = tex2Dlod(_HexCellData, float4(uv, 0, 0));
	data.w *= 255;
	return data;
}

float4 GetCellData(float2 cellDataCoordinates) {
	float2 uv = cellDataCoordinates + 0.5;
	uv.x *= _HexCellData_TexelSize.x;
	uv.y *= _HexCellData_TexelSize.y;
	return tex2Dlod(_HexCellData, float4(uv, 0, 0));
}