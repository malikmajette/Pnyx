%% Plot file variables
filename = 'C:\Users\malik\OneDrive\Documents\Pnyx\scaled_floor_60.csv';
p = csvread(filename,1);
x = p(:,1)*.01;
y = p(:,2)*.01;
z = p(:,3)*.01;
P = [x,y,z];
figure
plot3(x,z,y,'.','markersize',6)
xlabel('X')
ylabel('Z')
zlabel('Y')
title('Plot','fontweight','b');
grid on

%% Interpolate data using scatteredInterpolant
figure
[xi,yi] = meshgrid(-80:20:80,-10:10:70);
Pxz = [x,z];
%remove any duplicate points
[Pxz,ia,ic] = unique(Pxz,'rows','stable');
Py = y;
%remove any duplicate values
for i = 1:size(ic)
    if ic(i) < i
        Py(i) = [];
    break
    end
end
F = scatteredInterpolant(Pxz,Py,'linear','none');
surf(xi,yi,F(xi,yi));
title('Linear Interpolation Method','fontweight','b');

%% Initialize random generator range
[ix,iz] = meshgrid(0:.01:1,0:.01:1);
rx = min(x)+ix*(max(x)-min(x));
rz = min(z)+iz*(max(z)-min(z));

%% Plot test points
res = size(rx(:,1));
res = res(1);
rx = reshape(rx,res*res,1);
rz = reshape(rz,res*res,1);
r = [rx,rz];
ry = F(r);
figure
plot3(rx,rz,ry,'.','markersize',6)
grid on
title('Plot','fontweight','b');
points = [rx,ry,rz];
csvwrite('pnyx_test_points.txt',points);

%% Write OBJ file
fhandle = fopen('test_points_refined.obj', 'w');
for i = 1:res*res 
    %print vertices
    fprintf(fhandle, 'v %d %d %d \n', points(i,:));
end
for j = 1:res*res-(res+1)
    %print faces
    fprintf(fhandle, 'f %d %d %d %d\n', j, j+1, j+res+1, j+res);
end
fclose(fhandle);
