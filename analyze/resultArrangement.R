library(dplyr)
# Names
names = c("jg","yb","tj")
p_levels = c("0","10","20","30","40","50","60","70","80","90","100","110","120","130","140","150","160","170",
             "180","190","200","210","220","230","240","250","260","270","280","290","300","310","320","330","340","350")
# 1. 1 Letter Accuracy [%]  
base_df = data.frame()
for (i in 1:3){
  file_name = paste("data/",names[i],"_exp.csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  file_data$name = names[i]
  base_df = rbind(base_df,file_data)
}
base_df$name = factor(base_df$name, levels=names)
base_df$pattern = factor(base_df$pattern, levels=p_levels)

base_df_high = base_df[base_df$confidence==3,]
base_df_middle = base_df[base_df$confidence==2,]
base_df_low = base_df[base_df$confidence==1,]

nrow(base_df_high)
nrow(base_df_middle)
nrow(base_df_low)

a0 = base_df_high[base_df_high$pattern=="a0",]
a1 = base_df_high[base_df_high$pattern=="a1",]
a2 = base_df_high[base_df_high$pattern=="a2",]
a3 = base_df_high[base_df_high$pattern=="a3",]
a4 = base_df_high[base_df_high$pattern=="a4",]
a5 = base_df_high[base_df_high$pattern=="a5",]
a6 = base_df_high[base_df_high$pattern=="a6",]
a7 = base_df_high[base_df_high$pattern=="a7",]
a8 = base_df_high[base_df_high$pattern=="a8",]
a9 = base_df_high[base_df_high$pattern=="a9",]
a10 = base_df_high[base_df_high$pattern=="a10",]
a11 = base_df_high[base_df_high$pattern=="a11",]
a12 = base_df_high[base_df_high$pattern=="a12",]
a13 = base_df_high[base_df_high$pattern=="a13",]
a14 = base_df_high[base_df_high$pattern=="a14",]
a15 = base_df_high[base_df_high$pattern=="a15",]
a16 = base_df_high[base_df_high$pattern=="a16",]
a17 = base_df_high[base_df_high$pattern=="a17",]
b0 = base_df_high[base_df_high$pattern=="b0",]
b1 = base_df_high[base_df_high$pattern=="b1",]
b2 = base_df_high[base_df_high$pattern=="b2",]
b3 = base_df_high[base_df_high$pattern=="b3",]
b4 = base_df_high[base_df_high$pattern=="b4",]
b5 = base_df_high[base_df_high$pattern=="b5",]
b6 = base_df_high[base_df_high$pattern=="b6",]
b7 = base_df_high[base_df_high$pattern=="b7",]
b8 = base_df_high[base_df_high$pattern=="b8",]
b9 = base_df_high[base_df_high$pattern=="b9",]
b10 = base_df_high[base_df_high$pattern=="b10",]
b11 = base_df_high[base_df_high$pattern=="b11",]
b12 = base_df_high[base_df_high$pattern=="b12",]
b13 = base_df_high[base_df_high$pattern=="b13",]
b14 = base_df_high[base_df_high$pattern=="b14",]
b15 = base_df_high[base_df_high$pattern=="b15",]
b16 = base_df_high[base_df_high$pattern=="b16",]
b17 = base_df_high[base_df_high$pattern=="b17",]

a0
a1
a2
a3
a4
a5
a6
a7
a8
a9
a10
a11
a12
a13
a14
a15
a16
a17
b0
b1
b2
b3
b4
b5
b6
b7
b8
b9
b10
b11
b12
b13
b14
b15
b16
b17


high_out = group_by(base_df_high, name, pattern) %>%
  summarise(
    count = n()
    #sd = sd(correct, na.rm = TRUE)*100
  )
print(high_out,n=36)

high_out_df = as.data.frame(high_out)
ggplot(data=high_out_df, aes(x=pattern, y=count, fill=name))+
  ylim(0,3)+
  geom_bar(stat="identity", position=position_dodge(),width=0.5)

library(ggplot2)





nrow(base_df)

mean(base_df$correct)


out = group_by(base_df, name) %>%
  summarise(
    count = n(),
    mean = mean(correct, na.rm = TRUE)*100,
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
