library(dplyr)

# Names
names = c("p1","p2","p3","p4","p5","p6")
group = c("digit", "alphabet")
strategy = c("baseline","hetero")
armposture = c("armFront","armBody")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  

base_df = data.frame()
for (i in 1:6){
  for(j in 1:1){
    for(k in 1:2){
      for(p in 1:2){
        for(q in 1:1){
          file_name = paste("data2/",names[i] ,"_",group[j],"_",strategy[k],"_",armposture[p],"_", mode[q], ".csv",sep="")
          file_data = read.csv(file_name, header=T, stringsAsFactors = F)
          base_df = rbind(base_df,file_data)
        }
      }
    }
  }
}
base_df$id = factor(base_df$id, levels=names)
base_df$rt = base_df$enterstamp - base_df$playendstamp

result = group_by(base_df, id, group, strategy, armpose, mode) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100,
    #diff_1 = sum(diffCount_1),
    #diff_2 = sum(diffCount_2),
    #diff_3 = sum(diffCount_3),
    reactTime = mean(rt)
    #sd = sd(correct, na.rm = TRUE)*100
  )
print(result,n=100)
write.csv(result, "result.csv")
