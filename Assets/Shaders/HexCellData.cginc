sampler2D _HexCellData;
float4 _HexCellData_TexelSize;

float _Hex_InnerRadiusTimesTwo;
float _Hex_OuterRadiusTimesThree;

int GetCellIndexFromWorld(float3 worldPosition) {
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

		}else if(deltaZ > deltaY) {
			roundedZ = -roundedX - roundedY;
		}
	}

	return roundedX + roundedZ * _HexCellData_TexelSize.z + roundedZ / 2;
}

float4 GetCellData(int cellIndex) {
	float2 uv;
	uv.x = (cellIndex + 0.5) * _HexCellData_TexelSize.x;
	float row = floor(uv.x);
	uv.x -= row;
	uv.y = (row + 0.5) * _HexCellData_TexelSize.y;
	
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
