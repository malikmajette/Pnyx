figure
% Data
n = 8500;
radius = max(x);
xc = randn;
zc = randn;
% Engine
theta = rand(1,n)*(pi);
r = sqrt(rand(1,n))*radius;
xrand = xc + r.*cos(theta);
zrand = zc + r.*sin(theta);
xrand = xrand';
zrand = zrand';
i = 1;
j = 1;
while i <= length(xrand)-1
    while j <= length(xrand)-1
        if j ~= i
            test = sqrt((xrand(i)-xrand(j))^2 + (zrand(i)-zrand(j))^2);
            if test <= .45
                xrand(i) = [];
                zrand(i) = [];
                flag = true;
            else
                j=j+1;
            end
        else j=j+1;
        end 
    end
    j = 1;
    if flag
        flag = false;
    else i=i+1;
    end
end
plot(xrand,zrand,'s')
figure
yrand = F(xrand,zrand);
% Check
plot3(xrand,zrand,yrand,'s')
randPoints = [xrand,yrand,zrand];
i=1;
while i <= length(randPoints) 
    if (isnan(randPoints(i,2)))
        randPoints(i,:) = [];
    elseif (randPoints(i,3) < 14.5 && randPoints(i,1) > -14 && randPoints(i,1) < 14)
        randPoints(i,:) = [];   
    else
        i=i+1;
    end
end
csvwrite('pnyx_rand_points.txt',randPoints);