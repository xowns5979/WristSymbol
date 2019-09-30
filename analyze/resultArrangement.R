library(dplyr)


# Names
names = c("dh", "sb", "yb")

# 1. 1 Letter Accuracy [%]  
base_df = data.frame()
for (i in 1:3){
  file_name = paste("data/",names[i],"_exp.csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  file_data$name = names[i]
  base_df = rbind(base_df,file_data)
}
base_df$name = factor(base_df$name, levels=names)

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
