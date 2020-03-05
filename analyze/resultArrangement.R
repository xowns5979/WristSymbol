library(dplyr)


# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
cond = c("A(Baseline1)","B(Approach)","C(Baseline2)")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  

base_df = data.frame()
for (i in 1:12){
  file_name = paste("data/",names[i] ,"_",cond[1],"_",mode[2],".csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
}
base_df$id = factor(base_df$id, levels=names)

base_df

result = group_by(base_df, id) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100,
    rt = mean(reactionTime)
  )
print(result,n=36)
write.csv(result,"result.csv")


