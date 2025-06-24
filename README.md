# unillm
a framework to use llm for unity

# quick start
1. 拷贝Script中的所有文件到你的项目目录（如果有需要可以将其他部分也拷贝过去）
2. 配置API
	- 简要配置：框架默认使用qwen大模型，你只需要设置对应的环境变量（QWEN_API_KEY = YOUR_KEY，注意是用户的环境变量而非系统的环境变量）即可使用。
	- 复杂配置：你可以修改UnillmCommonAgentModelConfig的默认值，或者在创建Agent的时候传入自己想要的配置
	- 自定义：你可以自定义所需的Agent，只需要实现IUnillmAgent接口即可
