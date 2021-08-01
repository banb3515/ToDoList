#region API 참조
using System;
#endregion

namespace ToDoList.Models
{
    // 클래스 직렬화
    [Serializable()]
    public class ToDo
    {
        // 할 일 내용
        public string Content { get; set; }

        // 완료 여부
        public bool Done { get; set; }

        // 사진 경로
        public string Image { get; set; }

        #region 생성자
        public ToDo(string content)
        {
            Content = content;
            Done = false;
            Image = "Resources/NoDone_Button.png";
        }
        #endregion
    }
}