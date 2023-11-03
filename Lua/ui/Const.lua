local Const = {}

--- 加载状态
Const.LoadingState = {
    None = 0,
    Ready = 1,
    Loading = 2,
    Finished = 3,
    LoadDone = 4,
}

--- 事件
Const.EventID = {
    ButtonClicked = 0,
    SliderValueChanged = 1,
}

return Const