library(dplyr)


# Names
names = c("TJ","YB")
cond = c("Distal(withoutTip)", "Distal(withTip)", "Body(withoutTip)", "Body(withTip)")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  

base_df = data.frame()
for (i in 2:2){
  file_name = paste("Exp10_data/",names[i] ,"_",cond[4],"_",mode[2],".csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
}
base_df$id = factor(base_df$id, levels=names)
base_df

result = group_by(base_df, id) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100
    #diff_1 = sum(diffCount_1),
    #diff_2 = sum(diffCount_2),
    #diff_3 = sum(diffCount_3)
    #rt = mean(reaction_time)
    #sd = sd(correct, na.rm = TRUE)*100
  )
print(result,n=36)
