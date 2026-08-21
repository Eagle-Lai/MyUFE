using UnityEngine;
using System.Collections;

/// <summary>
/// 网格场地选择界面（GridStageSelectionScreen）。
/// <para>用途：以网格布局（每行 stagesPerRow 个场地）浏览场地列表的选场界面，</para>
/// <para>提供上/下/左/右移动光标的方法，并支持行列环绕导航。</para>
/// </summary>
public class GridStageSelectionScreen : StageSelectionScreen {
	#region public instance properties
	/// <summary>
	/// 网格总行数（按场地总数与每行数量向上取整计算）。
	/// </summary>
	public int numberOfRows{
		get{
			int totalStages = UFE.config.stages.Length;
			int rows = totalStages / this.stagesPerRow;
			
			if (totalStages % this.stagesPerRow != 0){
				++rows;
			}
			
			return rows;
		}
	}
	#endregion

	#region public instance properties
	/// <summary>移动光标时播放的音效。</summary>
	public AudioClip moveCursorSound;
	/// <summary>每行场地数量。</summary>
	public int stagesPerRow = 4;
	#endregion

	#region public instance methods
	/// <summary>
	/// 光标向下移动一行（底部环绕到顶部）。
	/// </summary>
	public virtual void MoveCursorDown(){
		// Retrieve the row and column of the stage
		int currentRow = this.stageHoverIndex / this.stagesPerRow;
		int currentColumn = this.stageHoverIndex % this.stagesPerRow;
		
		// Move the cursor to the left
		currentRow = (currentRow + 1) % this.numberOfRows;
		
		// Finally, update the position of the cursor
		this.MoveCursor(currentRow * this.stagesPerRow + currentColumn);
	}
	
	/// <summary>
	/// 光标向左移动一格（左端环绕到右端）。
	/// </summary>
	public virtual void MoveCursorLeft(){
		// Retrieve the row and column of the stage
		int currentRow = this.stageHoverIndex / this.stagesPerRow;
		int currentColumn = this.stageHoverIndex % this.stagesPerRow;
		
		// Move the cursor to the left
		currentColumn = (currentColumn + this.stagesPerRow - 1) % this.stagesPerRow;
		
		// Finally, update the position of the cursor
		this.MoveCursor(currentRow * this.stagesPerRow + currentColumn);
	}
	
	/// <summary>
	/// 光标向右移动一格（右端环绕到左端）。
	/// </summary>
	public virtual void MoveCursorRight(){
		// Retrieve the row and column of the stage
		int currentRow = this.stageHoverIndex / this.stagesPerRow;
		int currentColumn = this.stageHoverIndex % this.stagesPerRow;
		
		// Move the cursor to the left
		currentColumn = (currentColumn + 1) % this.stagesPerRow;
		
		// Finally, update the position of the cursor
		this.MoveCursor(currentRow * this.stagesPerRow + currentColumn);
	}
	
	/// <summary>
	/// 光标向上移动一行（顶部环绕到底部）。
	/// </summary>
	public virtual void MoveCursorUp(){
		// Retrieve the row and column of the stage
		int currentRow = this.stageHoverIndex / this.stagesPerRow;
		int currentColumn = this.stageHoverIndex % this.stagesPerRow;
		
		// Move the cursor to the left
		currentRow = (currentRow + this.numberOfRows - 1) % this.numberOfRows;
		
		// Finally, update the position of the cursor
		this.MoveCursor(currentRow * this.stagesPerRow + currentColumn);
	}
	#endregion
	
	#region protected instance methods
	/// <summary>
	/// 移动光标到指定索引（播放移动音效并更新悬停索引）。
	/// </summary>
	/// <param name="characterIndex">目标索引。</param>
	protected virtual void MoveCursor(int characterIndex){
		if (this.moveCursorSound != null) UFE.PlaySound(this.moveCursorSound);
		this.stageHoverIndex = characterIndex;
	}
	#endregion
}
