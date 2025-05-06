簡介:
本程式功能為每日自動驗證遊戲後端的弱連線與強連線功能是否運作正常。系統會依序進入各個遊戲並自動遊玩指定場次，同時記錄每局得分，並於遊玩後自動連線至後台BQ報表，驗證分數是否正確無誤。

前面是對 Form 物件的設定，可以直接從 Form1.cs 第 651 行進入
private async void button2_Click(object sender, EventArgs e){}
