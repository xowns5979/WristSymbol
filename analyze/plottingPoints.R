# 1. data frame making
exp_data = read.csv("data/yb/yb_exp.csv")
x = read.csv("data/yb/yb_touchdata.csv")
x$pattern = ""
x$startpoint = 0
x$endpoint = 0
count = 1

for (i in 1:nrow(x)){
  x[i,]$pattern = as.character(exp_data$pattern[count])
  if(x[i,]$pattern=="a(3)"){
    x[i,]$startpoint = 1
    x[i,]$endpoint = 2
  }
  else if (x[i,]$pattern=="b(4)"){
    x[i,]$startpoint = 1
    x[i,]$endpoint = 4
  }
  else if (x[i,]$pattern=="c(5)"){
    x[i,]$startpoint = 1 
      x[i,]$endpoint = 3
  }
  
  else if (x[i,]$pattern=="d(5)"){
    x[i,]$startpoint = 2
    x[i,]$endpoint = 4
  }
  
  else if (x[i,]$pattern=="e(6)"){
    x[i,]$startpoint = 2 
    x[i,]$endpoint = 3
  }
  
  else if (x[i,]$pattern=="f(7)"){
    x[i,]$startpoint = 2 
    x[i,]$endpoint = 1
  }
  
  else if (x[i,]$pattern=="g(7)"){
    x[i,]$startpoint = 4
    x[i,]$endpoint = 3
  }
  
  else if (x[i,]$pattern=="h(0)"){
    x[i,]$startpoint = 4 
    x[i,]$endpoint = 1
  }
  
  else if (x[i,]$pattern=="i(1)"){
    x[i,]$startpoint = 4 
    x[i,]$endpoint = 2
  }
  
  else if (x[i,]$pattern=="j(1)"){
    x[i,]$startpoint = 3 
    x[i,]$endpoint = 1
  }
  
  else if (x[i,]$pattern=="k(2)"){
    x[i,]$startpoint = 3 
    x[i,]$endpoint = 2
  }
  
  else if (x[i,]$pattern=="l(3)"){
    x[i,]$startpoint = 3 
    x[i,]$endpoint = 4
  }
  
  if (x[i,]$action == "up")
  {
    count = count + 1
  }
}

#2. plot (only) hand image
library(imager)
image <- load.image("data/yb/yb_aligned.png")
plot(image, axes=0)

color_set = c("#E23838","#F78200","#973999","#5EBD3E")

#3. plot localization data
#3-1. plot all points grouped by the true actuator position
for (i in 1:nrow(x)){
  if(x[i,]$action=="down")
  {
    color = color_set[x[i,]$startpoint]
    points(x[i,]$x, x[i,]$y, col=color, cex=0.6, pch=16)
  }
  else if (x[i,]$action=="up")
  {
    if(x[i,]$endpoint == 1)
      color = "#E23838"
    else if(x[i,]$endpoint == 2)
      color = "#F78200"
    else if(x[i,]$endpoint == 3)
      color = "#973999"
    else if(x[i,]$endpoint == 4)
      color = "#5EBD3E"
    
    points(x[i,]$x, x[i,]$y, col=color, cex=0.6, pch=16)
  }
}

#3-2. plot mean localization point grouped by the true actuator position
# distance from true actuator (picked for mean-2sd ~ mean+2sd)
#For geunwoo
#1(912,510)  2(1188,490)
#3(934, 784) 4(1220,748)
#actuators_x = c(912, 1188, 934, 1220)
#actuators_y = c(510, 490, 784, 748)

#For sunmin
#1(1420,602)  2(1680, 628)
#3(1380, 856) 4(1656, 880)

#For youngbo
#1(1178,614)  2(1446,636)
#3(1150,870) 4(1426,882)
#actuators_x = c(1178, 1446, 1150, 1426)
#actuators_y = c(614, 636, 870, 882)

actuators_x = c(1420, 1680, 1380, 1656)
actuators_y = c(602, 628, 856, 880)

actuators_x_perceived = c()
actuators_y_perceived = c()

library(raster)

for (target_actuator in 1:4){
  x2 = x[x$startpoint==target_actuator,]
  x_point_start = x2[x2$action=="down",]$x
  y_point_start = x2[x2$action=="down",]$y
  x3 = x[x$endpoint==target_actuator,]
  x_point_end = x3[x3$action=="up",]$x
  y_point_end = x3[x3$action=="up",]$y
  x_point = c(x_point_start, x_point_end)
  y_point = c(y_point_start, y_point_end)
  distance = c()
  for (i in 1:length(x_point)){
    pd = pointDistance(c(actuators_x[target_actuator],
                         actuators_y[target_actuator]),
                       c(x_point[i], y_point[i]),FALSE)
    distance = c(distance, pd)
  }
  
  distance_df = data.frame("x_point"=x_point, "y_point"=y_point, "distance"=distance)
  dist_sd = sd(distance_df$distance)
  dist_mean = mean(distance_df$distance)
  
  distance_df2=distance_df[distance_df$distance <= dist_mean+dist_sd,]
  
  final_x = mean(distance_df2$x_point)
  final_y = mean(distance_df2$y_point)
  
  actuators_x_perceived = c(actuators_x_perceived, final_x)
  actuators_y_perceived = c(actuators_y_perceived, final_y)
}

plot(image, axes=0)
for (target_actuator in 1:4){
  points(actuators_x_perceived[target_actuator],
         actuators_y_perceived[target_actuator],
         col=color_set[target_actuator],
         cex=2, pch=16)

}

#3-3. plot points for each case, also see the starting / end point by distinguished color?(or arrow?)

target_case = "l(3)"
x2 = x[x$pattern==target_case,]
start_x = -1
start_y = -1
end_x = -1
end_y = -1

plot(image, axes=0)
for (i in 1:nrow(x2)){
  if(x2[i,]$action=="down")
  {
    color = color_set[1]
    start_x = x2[i,]$x
    start_y = x2[i,]$y
    points(x2[i,]$x, x2[i,]$y, col=color, cex=1.3, pch=16)
  }
  else if (x2[i,]$action=="up")
  {
    color = color_set[2]
    end_x = x2[i,]$x
    end_y = x2[i,]$y
    points(x2[i,]$x, x2[i,]$y, col=color, cex=1.3, pch=16)
    arrows(start_x, start_y, end_x, end_y, length=0.15, angle=30, lty=1, lwd=2, col="green")
  }
}







