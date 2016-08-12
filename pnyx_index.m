[zr,xr] = meshgrid(-10:1:70,-65:1.625:65);
xr = reshape(xr, 6561, 1);
zr = reshape(zr, 6561, 1);
yr = F(xr,zr);
reference = [xr,yr,zr];
i = 1;
while i <= length(reference) 
    if (isnan(reference(i,2)))
        reference(i,:) = [1000,1000,1000];
    else i=i+1;
    end
end
figure, 
plot3(xr,zr,yr,'.');
grid on;
title('Spatial Index Plot','fontweight','b')
csvwrite('pnyx_reference_points.txt',reference);