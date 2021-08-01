#region API 참조
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using ToDoList.Models;
#endregion

namespace ToDoList
{
    public partial class MainWindow : Window
    {
        #region 변수
        /*
         * 패널 0 : 할 일 목록
         * 패널 1 : 완료된 할 일 목록
         */
        // 현재 보여지고 있는 패널
        int panel = 0;

        // 애니메이션 작동 중 확인
        bool animation = false;

        // 할 일 목록
        List<ToDo> toDoList = new List<ToDo>();

        // 완료된 할 일 목록
        List<ToDo> doneList = new List<ToDo>();
        #endregion

        #region 생성자
        public MainWindow()
        {
            InitializeComponent();

            // 데이터 파일로부터 데이터 읽어와서 변수에 저장
            var fi = new FileInfo("data\\to_do_list.data");
            if (fi.Exists)
            {
                var formatter = new BinaryFormatter();
                var stream = File.Open("data\\to_do_list.data", FileMode.Open);
                toDoList = (List<ToDo>)formatter.Deserialize(stream);
            }

            fi = new FileInfo("data\\done_list.data");
            if (fi.Exists)
            {
                var formatter = new BinaryFormatter();
                var stream = File.Open("data\\done_list.data", FileMode.Open);
                doneList = (List<ToDo>)formatter.Deserialize(stream);
            }

            // 할 일 목록 새로고침
            ToDoListRefresh();
        }
        #endregion

        #region 프로그램이 종료될 때
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 현재 데이터를 파일로 저장
            var formatter = new BinaryFormatter();
            var di = new DirectoryInfo("data");
            if (!di.Exists)
                di.Create();

            var stream = File.Open("data\\to_do_list.data", FileMode.Create);
            formatter.Serialize(stream, toDoList);
            stream.Close();

            stream = File.Open("data\\done_list.data", FileMode.Create);
            formatter.Serialize(stream, doneList);
            stream.Close();
        }
        #endregion

        #region 할 일 목록 새로고침
        private void ToDoListRefresh()
        {
            if (EmptyList.Visibility == Visibility.Visible)
                EmptyList.Visibility = Visibility.Hidden;

            // 패널 0이 보여지고 있을 때, 할 일 목록 보여줌
            if (panel == 0 && toDoList.Count > 0)
            {
                ToDoListItems.ItemsSource = toDoList;
                ToDoListItems.Items.Refresh();
                return;
            }
            // 패널 1이 보여지고 있을 때, 완료된 할 일 목록 보여줌
            else if (panel == 1 && doneList.Count > 0)
            {
                ToDoListItems.ItemsSource = doneList;
                ToDoListItems.Items.Refresh();
                return;
            }

            // 할 일 목록 또는 완료된 할 일 목록이 비어있을 때
            ToDoListItems.ItemsSource = new List<ToDo>();
            EmptyList.Visibility = Visibility.Visible;
        }
        #endregion

        #region 클릭 애니메이션
        private void ClickAnimation(System.Windows.Controls.Label label)
        {
            // 기본 배율 X: 1, Y: 1
            double scaleX = 1;
            double scaleY = 1;

            // 애니메이션 타이머
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += new EventHandler((s, ea) =>
            {
                // 1 Milliseconds 마다 0.02배율 씩 감소
                var sTrans = new ScaleTransform(scaleX, scaleY,
                    label.ActualWidth / 2, label.ActualHeight / 2);
                label.RenderTransform = sTrans;
                
                scaleX -= .02;
                scaleY -= .02;

                // 배율이 0.75 이하가 되면 기본 배율로 되돌림
                if (scaleX < 0.75 || scaleY < 0.75)
                {
                    timer.Stop();

                    var timer2 = new DispatcherTimer();
                    timer2.Interval = TimeSpan.FromMilliseconds(1);
                    timer2.Tick += new EventHandler((s2, ea2) =>
                    {
                        sTrans = new ScaleTransform(scaleX, scaleY,
                            label.ActualWidth / 2, label.ActualHeight / 2);
                        label.RenderTransform = sTrans;
                        scaleX += .02;
                        scaleY += .02;
                        if (scaleX == 1 || scaleY == 1)
                            timer2.Stop();
                    });
                    timer2.Start();
                }
            });
            timer.Start();
        }
        #endregion

        #region 할 일 추가 버튼
        private void AddButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 애니메이션 중복 방지
            if (!animation)
            {
                animation = true;
                ClickAnimation(sender as System.Windows.Controls.Label);
                animation = false;
            }

            // 할 일 추가 팝업창을 보여줌
            if(AddPopup.Visibility == Visibility.Hidden)
            {
                AddPopup.Visibility = Visibility.Visible;
                AddPopup.IsEnabled = true;

                PopupContent.Focus();
            }
        }
        #endregion

        #region 할 일 추가 - 숨기기 버튼
        private void HideButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 할 일 추가 팝업 숨기기
            AddPopup.Visibility = Visibility.Hidden;
            AddPopup.IsEnabled = false;
            PopupContent.Text = "";
        }
        #endregion

        #region 추가 완료 버튼 클릭
        private void AddDoneButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 할 일 목록 맨 처음에 추가
            toDoList.Insert(0, new ToDo(PopupContent.Text.Trim()));

            AddPopup.Visibility = Visibility.Hidden;
            AddPopup.IsEnabled = false;
            PopupContent.Text = "";

            ToDoListRefresh();
        }
        #endregion

        #region 할 일 목록 버튼 클릭
        private void ToDoListButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 애니메이션 중복 방지
            if(!animation)
            {
                animation = true;
                ClickAnimation(sender as System.Windows.Controls.Label);
                ToDoListButtonBackground.Visibility = Visibility.Visible;
                DoneListButtonBackground.Visibility = Visibility.Hidden;
                animation = false;
            }

            // 패널 0으로 변경
            if (panel != 0)
            {
                panel = 0;
                ToDoListRefresh();
            }
        }
        #endregion

        #region 완료된 할 일 목록 버튼 클릭
        private void DoneListButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 애니메이션 중복 방지
            if(!animation)
            {
                animation = true;
                ClickAnimation(sender as System.Windows.Controls.Label);
                DoneListButtonBackground.Visibility = Visibility.Visible;
                ToDoListButtonBackground.Visibility = Visibility.Hidden;
                animation = false;
            }

            // 패널 1로 변경
            if (panel != 1)
            {
                panel = 1;
                ToDoListRefresh();
            }
        }
        #endregion

        #region 완료로 표시/완료로 표시 해제 버튼 클릭
        private void DoneButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 완료로 표시 버튼 클릭 시 ToDo.Done 값을 true, ToDo.Image 값을 Done_Button.png로 변경 후
            // 할 일 목록에서 삭제 후 완료된 할 일 목록에 추가
            if (panel == 0)
            {
                for (int i = 0; i < toDoList.Count; i++)
                {
                    if (toDoList[i].Content == (sender as System.Windows.Controls.Label).Content.ToString())
                    {
                        toDoList[i].Done = true;
                        toDoList[i].Image = "Resources/Done_Button.png";
                        doneList.Add(toDoList[i]);
                        toDoList.RemoveAt(i);
                        break;
                    }
                }
            }
            // 완료로 표시 해제 버튼 클릭 시 ToDo.Done 값을 false, ToDo.Image 값을 NoDone_Button.png로 변경 후
            // 완료된 할 일 목록에서 삭제 후 할 일 목록에 추가
            else if (panel == 1)
            {
                for (int i = 0; i < doneList.Count; i++)
                {
                    if (doneList[i].Content == (sender as System.Windows.Controls.Label).Content.ToString())
                    {
                        doneList[i].Done = false;
                        doneList[i].Image = "Resources/NoDone_Button.png";
                        toDoList.Add(doneList[i]);
                        doneList.RemoveAt(i);
                        break;
                    }
                }
            }

            ToDoListRefresh();
        }
        #endregion

        #region 할 일 삭제 버튼
        private void DeleteButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (panel == 0)
            {
                for (int i = 0; i < toDoList.Count; i++)
                {
                    if (toDoList[i].Content == (sender as System.Windows.Controls.Label).Content.ToString())
                    {
                        toDoList.RemoveAt(i);
                        break;
                    }
                }
            }
            else if(panel == 1)
            {
                for (int i = 0; i < doneList.Count; i++)
                {
                    if (doneList[i].Content == (sender as System.Windows.Controls.Label).Content.ToString())
                    {
                        doneList.RemoveAt(i);
                        break;
                    }
                }
            }

            ToDoListRefresh();
        }
        #endregion
    }
}