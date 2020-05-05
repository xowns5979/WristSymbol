library(dplyr)


# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10")
cond = c("armFront","armBody")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  

base_df = data.frame()
for(j in 2:2)
{
  for (i in 1:10){
    file_name = paste("Exp14_data/",names[i] ,"_",cond[j],"_",mode[2],".csv",sep="")
    file_data = read.csv(file_name, header=T, stringsAsFactors = F)
    base_df = rbind(base_df,file_data)
  }  
}

base_df$id = factor(base_df$id, levels=names)
base_df

result = group_by(base_df, cond) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100,
    c1 = mean(c1)*100,
    c2 = mean(c2)*100,
    c3 = mean(c3)*100,
    reactionTime = mean(reactionTime)
    #diff_1 = sum(diffCount_1),
    #diff_2 = sum(diffCount_2),
    #diff_3 = sum(diffCount_3)
    #rt = mean(reaction_time)
    #sd = sd(correct, na.rm = TRUE)*100
  )
print(result,n=36)
write.csv(result, "result.csv")
