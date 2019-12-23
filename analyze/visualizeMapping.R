library(dplyr)
# Names
names = c("jg","yb","tj")
#names = c("tj")
p_levels = c("0","10","20","30","40","50","60","70","80","90",
            "100","110","120","130","140","150","160","170","180","190",
            "200","210","220","230","240","250","260","270","280","290",
            "300","310","320","330","340","350")
# 1. 1 Letter Accuracy [%]  
base_df = data.frame()
for (i in 1:3){
#for (i in 1:1){
  file_name = paste("data/",names[i],"_exp.csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  #file_data$name = names[i]
  base_df = rbind(base_df,file_data)
}
# 
# for (i in 1:nrow(base_df)){
#   if(base_df[i,]$answer == 0)
#     base_df[i,]$answer = 0
#   else if(base_df[i,]$answer == 1)
#     base_df[i,]$answer = 7
#   else if(base_df[i,]$answer == 2)
#     base_df[i,]$answer = 6
#   else if(base_df[i,]$answer == 3)
#     base_df[i,]$answer = 5
#   else if(base_df[i,]$answer == 4)
#     base_df[i,]$answer = 4
#   else if(base_df[i,]$answer == 5)
#     base_df[i,]$answer = 3
#   else if(base_df[i,]$answer == 6)
#     base_df[i,]$answer = 2
#   else if(base_df[i,]$answer == 7)
#     base_df[i,]$answer = 1
# }
# 
# write.csv(base_df,"tj_exp.csv",row.names=FALSE)
# 





base_df$name = factor(base_df$name, levels=names)
base_df$pattern = factor(base_df$pattern, levels=p_levels)



base_df_high = base_df[base_df$confidence==3,]
base_df_middle = base_df[base_df$confidence==2,]
base_df_low = base_df[base_df$confidence==1,]





base_df_person = base_df[base_df$name=="tj",]
draw_func <- function(){
  pattern_x = c(0,10,20,30,40,50,60,70,80,90,100,110,120,130,140,150,160,170,
                180,190,200,210,220,230,240,250,260,270,280,290,300,310,320,
                330,340,350)
  pattern_y = c(70,70,70,70,70,70,70,70,70,70,70,70,70,70,70,70,70,70,
                70,70,70,70,70,70,70,70,70,70,70,70,70,70,70,70,70,70)
  pattern_labels = c("0","10","20","30","40","50","60","70","80","90",
                     "100","110","120","130","140","150","160","170","180","190",
                     "200","210","220","230","240","250","260","270","280","290",
                     "300","310","320","330","340","350")
  response_x = c(0, 45,90,135,180,225,270,315)
  response_y = c(0,0,0,0,0,0,0,0)
  response_labels = c("0","45","90","135","180","225","270","315")
  plot.new(); plot.window(xlim=c(0,350),ylim=c(0,100) )
  lines (c(0,350), c(70,70), type='l',col='black', lwd=3) # line for pattern
  lines (c(0,350), c(0,0), type='l',col='black', lwd=3) # line for response 
  points(pattern_x, pattern_y,cex=1.5,pch=16)
  points(response_x, response_y,cex=1.5,pch=16)
  text(x=pattern_x, y=pattern_y, labels=pattern_labels, pos = 3, cex = 0.8)
  text(x=response_x, y=response_y, labels=response_labels, pos = 1, cex = 0.8)
  
  transparency_mid = "20"
  transparency_high = "60"
  color_set_mid = c(paste("#FF0000",transparency_mid,sep=""),paste("#FF8000",transparency_mid,sep=""),
                    paste("#FFFF00",transparency_mid,sep=""),paste("#00FF00",transparency_mid,sep=""),
                    paste("#0080FF",transparency_mid,sep=""),paste("#0000FF",transparency_mid,sep=""),
                    paste("#7F00FF",transparency_mid,sep=""),paste("#FF00FF",transparency_mid,sep=""))
  color_set_high = c(paste("#FF0000",transparency_high,sep=""),paste("#FF8000",transparency_high,sep=""),
                     paste("#FFFF00",transparency_high,sep=""),paste("#00FF00",transparency_high,sep=""),
                     paste("#0080FF",transparency_high,sep=""),paste("#0000FF",transparency_high,sep=""),
                     paste("#7F00FF",transparency_high,sep=""),paste("#FF00FF",transparency_high,sep=""))
  for (i in 1:nrow(base_df_person)){
    line_x = c(as.numeric(as.character(base_df_person[i,]$pattern)), response_x[base_df_person[i,]$answer+1])
    line_y = c(70, 0)
    color = ""
    if(base_df_person[i,]$confidence==2)
    {
      if(base_df_person[i,]$answer == 0)
        color = color_set_mid[1]
      else if(base_df_person[i,]$answer == 1)
        color = color_set_mid[2]
      else if(base_df_person[i,]$answer == 2)
        color = color_set_mid[3]
      else if(base_df_person[i,]$answer == 3)
        color = color_set_mid[4]
      else if(base_df_person[i,]$answer == 4)
        color = color_set_mid[5]
      else if(base_df_person[i,]$answer == 5)
        color = color_set_mid[6]
      else if(base_df_person[i,]$answer == 6)
        color = color_set_mid[7]
      else if(base_df_person[i,]$answer == 7)
        color = color_set_mid[8]
      
      lines(line_x,line_y, type='l', col=color ,lwd='12')
    }
    else if(base_df_person[i,]$confidence==3)
    {
      if(base_df_person[i,]$answer == 0)
        color = color_set_high[1]
      else if(base_df_person[i,]$answer == 1)
        color = color_set_high[2]
      else if(base_df_person[i,]$answer == 2)
        color = color_set_high[3]
      else if(base_df_person[i,]$answer == 3)
        color = color_set_high[4]
      else if(base_df_person[i,]$answer == 4)
        color = color_set_high[5]
      else if(base_df_person[i,]$answer == 5)
        color = color_set_high[6]
      else if(base_df_person[i,]$answer == 6)
        color = color_set_high[7]
      else if(base_df_person[i,]$answer == 7)
        color = color_set_high[8]
      
      lines(line_x,line_y, type='l', col=color ,lwd='12')
    }
  }
}
draw_func()



paste("15","32",sep="")
"15" + "32"
i=2
line_x = c(as.numeric(as.character(base_df[i,]$pattern)), response_x[base_df[i,]$answer])
line_y = c(70, 10)
line_x
line_y



3






out = group_by(base_df, pattern) %>%
  summarise(
    count = n(),
    #sd = sd(correct, na.rm = TRUE)*100
  )
out

out = group_by(base_df, pattern) %>%
  summarise(
    count = n(),
    mean = mean(correct, na.rm = TRUE)*100,
    #sd = sd(correct, na.rm = TRUE)*100
  )
out
