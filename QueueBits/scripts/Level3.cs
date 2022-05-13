using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using System.Threading;

//using System;
//using MySql.Data.MySqlClient;
//using Data;

namespace QueueBits
{
	public class Level3 : MonoBehaviour
	{
		enum Piece
		{
			Empty = 0,
			Blue = 1,
			Red = 2
		}

		[Range(3, 8)]
		public int numRows = 6;
		[Range(3, 8)]
		public int numColumns = 7;

		[Tooltip("How many pieces have to be connected to win.")]
		public int numPiecesToWin = 4;

		[Tooltip("Allow diagonally connected Pieces?")]
		public bool allowDiagonally = true;

		public float dropTime = 4f;

		// Gameobjects 
		public GameObject pieceRed;
		public GameObject pieceBlue;
		public GameObject pieceField;
		public GameObject finalColor;

		public GameObject pieceSuperposition;
		public GameObject piece25;
		public GameObject piece50;
		public GameObject piece75;
		public GameObject playerTurnText;

		//Piece Count Displays
		public GameObject blueTitle;
		public GameObject redTitle;

		//BLUE
		public GameObject pieceBlue100;
		public GameObject pieceBlue75;
		public GameObject pieceBlue50;
		public GameObject pieceBlue25;

		public GameObject pieceBlue100Text;
		public GameObject pieceBlue75Text;
		public GameObject pieceBlue50Text;
		public GameObject pieceBlue25Text;

		public GameObject pieceCounterText;

		//RED
		public GameObject pieceRed100;
		public GameObject pieceRed75;
		public GameObject pieceRed50;
		public GameObject pieceRed25;

		public GameObject pieceRed100Text;
		public GameObject pieceRed75Text;
		public GameObject pieceRed50Text;
		public GameObject pieceRed25Text;

		public int probability;

		Dictionary<int, (int, (int, int))> probDict = new Dictionary<int, (int, (int, int))>();

		Dictionary<int, int> redProbs = new Dictionary<int, int>();
		Dictionary<int, int> blueProbs = new Dictionary<int, int>();

		int bturns = 0;

		public GameObject winningText;

		public GameObject playerTurnObject;

		public string playerWonText = "Yellow Won!";
		public string playerLoseText = "Red Won!";
		public string drawText = "Draw!";

		public GameObject probText;

		public GameObject btnPlayAgain;
		bool btnPlayAgainTouching = false;
		Color btnPlayAgainOrigColor;
		Color btnPlayAgainHoverColor = new Color(255, 143, 4);

		GameObject gameObjectField;

		// temporary gameobject, holds the piece at mouse position until the mouse has clicked
		GameObject gameObjectTurn;

		/// <summary>
		/// The Game field.
		/// 0 = Empty
		/// 1 = Blue
		/// 2 = Red
		/// </summary>
		int[,] field;

		bool isPlayersTurn = true;
		bool isLoading = true;
		bool isDropping = false;
		bool mouseButtonPressed = false;

		bool gameOver = false;
		bool isCheckingForWinner = false;

		string state = "000000000000000000000000000000000000000000";
		int[] colPointers = { 5, 5, 5, 5, 5, 5, 5 };
		HashSet<(int, int)> visited = new HashSet<(int, int)>();

		// Use this for initialization
		void Start()
		{
			redProbs.Add(50, 6);
			redProbs.Add(75, 7);
			redProbs.Add(100, 8);

			blueProbs.Add(50, 6);
			blueProbs.Add(75, 7);
			blueProbs.Add(100, 8);

			int max = Mathf.Max(numRows, numColumns);

			if (numPiecesToWin > max)
				numPiecesToWin = max;

			CreateField();

			isPlayersTurn = System.Convert.ToBoolean(Random.Range(0, 2));
			playerTurnText.GetComponent<TextMesh>().text = isPlayersTurn ? "Yellow's Turn" : "Red's Turn";

			if (isPlayersTurn)
			{
				playerTurnObject = Instantiate(pieceBlue, new Vector3(numColumns - 1.75f, -6, 1), Quaternion.identity) as GameObject;
				playerTurnObject.transform.localScale -= new Vector3(0.5f, 0.5f, 0);
			}
			else
			{
				playerTurnObject = Instantiate(pieceRed, new Vector3(numColumns - 2.25f, -6, 1), Quaternion.identity) as GameObject;
				playerTurnObject.transform.localScale -= new Vector3(0.5f, 0.5f, 0);
			}

			btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;
		}

		/// <summary>
		/// Creates the field.
		/// </summary>
		void CreateField()
		{
			winningText.SetActive(false);
			btnPlayAgain.SetActive(true);
			playerTurnText.SetActive(true);
			playerTurnText.GetComponent<Renderer>().sortingOrder = 4;

			isLoading = true;

			gameObjectField = GameObject.Find("Field");
			if (gameObjectField != null)
			{
				DestroyImmediate(gameObjectField);
			}
			gameObjectField = new GameObject("Field");

			// create an empty field and instantiate the cells
			field = new int[numColumns, numRows];
			for (int x = 0; x < numColumns; x++)
			{
				for (int y = 0; y < numRows; y++)
				{
					field[x, y] = (int)Piece.Empty;
					GameObject g = Instantiate(pieceField, new Vector3(x, -y, -1), Quaternion.identity) as GameObject;
					g.transform.parent = gameObjectField.transform;
				}
			}

			isLoading = false;
			gameOver = false;

			// center camera
			Camera.main.transform.position = new Vector3(
				(numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f), Camera.main.transform.position.z);

			winningText.transform.position = new Vector3(
				(numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) + 1, winningText.transform.position.z);

			//btnPlayAgain.transform.position = new Vector3(
			//	(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) - 1, btnPlayAgain.transform.position.z);

			playerTurnText.transform.position = new Vector3(
			(numColumns - 1) / 2.0f, -6, playerTurnText.transform.position.z);

			//Piece Count Displays
			blueTitle.transform.position = new Vector3(-3, 1, 0);
			blueTitle.GetComponent<Renderer>().sortingOrder = 10;
			blueTitle.SetActive(true);

			redTitle.transform.position = new Vector3(6.75f, 1, 0);
			redTitle.GetComponent<Renderer>().sortingOrder = 10;
			redTitle.SetActive(true);

			pieceBlue100 = Instantiate(pieceBlue, new Vector3(-1, 0, -1), Quaternion.identity) as GameObject;
			pieceBlue100.transform.localScale -= new Vector3(0.5f, 0.5f, 0);

			pieceBlue75 = Instantiate(piece75, new Vector3(-1, -1, -1), Quaternion.identity) as GameObject;
			pieceBlue75.transform.localScale -= new Vector3(0.5f, 0.5f, 0);

			pieceBlue50 = Instantiate(piece50, new Vector3(-1, -2, -1), Quaternion.identity) as GameObject;
			pieceBlue50.transform.localScale -= new Vector3(0.5f, 0.5f, 0);

			//Piece Count Texts - BLUE
			pieceBlue100Text = Instantiate(pieceCounterText, new Vector3(-1.75f, 0, -1), Quaternion.identity) as GameObject;
			pieceBlue100Text.GetComponent<TextMesh>().text = blueProbs[100].ToString();
			pieceBlue100Text.SetActive(true);

			pieceBlue75Text = Instantiate(pieceCounterText, new Vector3(-1.75f, -1, -1), Quaternion.identity) as GameObject;
			pieceBlue75Text.GetComponent<TextMesh>().text = blueProbs[75].ToString();
			pieceBlue75Text.SetActive(true);

			pieceBlue50Text = Instantiate(pieceCounterText, new Vector3(-1.75f, -2, -1), Quaternion.identity) as GameObject;
			pieceBlue50Text.GetComponent<TextMesh>().text = blueProbs[50].ToString();
			pieceBlue50Text.SetActive(true);


			//Piece Count Displays - RED
			pieceRed100 = Instantiate(pieceRed, new Vector3(7, 0, -1), Quaternion.identity) as GameObject;
			pieceRed100.transform.localScale -= new Vector3(0.5f, 0.5f, 0);

			pieceRed75 = Instantiate(piece25, new Vector3(7, -1, -1), Quaternion.identity) as GameObject;
			pieceRed75.transform.localScale -= new Vector3(0.5f, 0.5f, 0);

			pieceRed50 = Instantiate(piece50, new Vector3(7, -2, -1), Quaternion.identity) as GameObject;
			pieceRed50.transform.localScale -= new Vector3(0.5f, 0.5f, 0);


			//Piece Count Texts - RED
			pieceRed100Text = Instantiate(pieceCounterText, new Vector3(7.5f, 0, -1), Quaternion.identity) as GameObject;
			pieceRed100Text.GetComponent<TextMesh>().text = redProbs[100].ToString();
			pieceRed100Text.SetActive(true);

			pieceRed75Text = Instantiate(pieceCounterText, new Vector3(7.5f, -1, -1), Quaternion.identity) as GameObject;
			pieceRed75Text.GetComponent<TextMesh>().text = redProbs[75].ToString();
			pieceRed75Text.SetActive(true);

			pieceRed50Text = Instantiate(pieceCounterText, new Vector3(7.5f, -2, -1), Quaternion.identity) as GameObject;
			pieceRed50Text.GetComponent<TextMesh>().text = redProbs[50].ToString();
			pieceRed50Text.SetActive(true);

			pieceCounterText.SetActive(false);
		}

		int index(int r, int c)
		{
			int i = r * 7 + c;
			return i;
		}

		(int, int) reverse_index(int i)
		{
			int r = i / 7;
			int c = i % 7;
			return (r, c);
		}

		void printState(string state)
		{
			string p_state = "";
			for (int r = 0; r < 6; r++)
			{
				int i = index(r, 0);
				p_state += state.Substring(i, 7) + "\n";
			}
			Debug.Log(p_state);
		}

		int evaluateState(string state)
		{
			int score = 0;
			visited.Clear();

			for (int r = 0; r < 6; r++)
			{
				for (int c = 0; c < 7; c++)
				{
					int i = index(r, c);
					if (state.Substring(i, 1).Equals("2")) // && !visited.Contains((r,c)) -- adding this causes missed connections, but speed?
					{
						score += evaluatePosition(state, r, c, "2");
					}
					else if (state.Substring(i, 1).Equals("1"))
					{
						score -= 2 * evaluatePosition(state, r, c, "1");
					}
				}
			}
			return score;
		}

		int evaluatePosition(string state, int r, int c, string color)
		{
			int i = index(r, c);
			(int, int) pos;

			int[] num_connected = new int[4];
			int hasCenter = 0;

			// look right
			int r_counter = 0;
			while (i < state.Length && state.Substring(i, 1).Equals(color))
			{
				pos = reverse_index(i);
				visited.Add(pos);

				(int row, int col) = pos;
				if (col >= 2 && col <= 4)
					hasCenter = 1;

				r_counter++;
				i++;
			}
			r_counter = Mathf.Min(4, r_counter); // if 5 or more connected, goes to 4.
			num_connected[r_counter - 1]++;

			// look down
			i = index(r, c);
			int d_counter = 0;
			while (i < state.Length && state.Substring(i, 1).Equals(color))
			{
				pos = reverse_index(i);
				visited.Add(pos);

				(int row, int col) = pos;
				if (col >= 2 && col <= 4)
					hasCenter = 1;

				d_counter++;
				i += 7;
			}
			d_counter = Mathf.Min(4, d_counter);
			num_connected[d_counter - 1]++;

			// look diagonal right-down
			i = index(r, c);
			int rd_counter = 0;
			while (i < state.Length && state.Substring(i, 1).Equals(color))
			{
				pos = reverse_index(i);
				visited.Add(pos);

				(int row, int col) = pos;
				if (col >= 2 && col <= 4)
					hasCenter = 1;

				rd_counter++;
				i += 8;
			}
			rd_counter = Mathf.Min(4, rd_counter);
			num_connected[rd_counter - 1]++;

			// look diagonal left-down
			i = index(r, c);
			int ld_counter = 0;
			while (i < state.Length && state.Substring(i, 1).Equals(color))
			{
				pos = reverse_index(i);
				visited.Add(pos);

				(int row, int col) = pos;
				if (col >= 2 && col <= 4)
					hasCenter = 1;

				ld_counter++;
				i += 6;
			}
			ld_counter = Mathf.Min(4, ld_counter);
			num_connected[ld_counter - 1]++;

			int score = 100 * num_connected[3] + 20 * num_connected[2] + 3 * num_connected[1] + 10 * hasCenter;
			return score;
		}

		List<int> getMoves(int[] cols)
		{
			List<int> possCols = new List<int>();
			for (int i = 0; i < cols.Length; i++)
			{
				if (cols[i] != -1)
					possCols.Add(i);
			}
			return possCols;
		}

		void playMove(int column, string color)
		{
			int r = colPointers[column];
			colPointers[column] -= 1;

			int i = index(r, column);

			if (i == 0)
				state = color + state.Substring(1); //prevents substring with length 0 error
			else
				state = state.Substring(0, i) + color + state.Substring(i + 1);
		}

		void reverseMove(int column)
		{
			colPointers[column] += 1;
			int r = colPointers[column];

			int i = index(r, column);

			if (i == 0)
				state = "0" + state.Substring(1); //prevents substring with length 0 error
			else
			{
				state = state.Substring(0, i) + "0" + state.Substring(i + 1);
			}

		}

		int findBestMove(string state, int[] cols)
		{
			int bestVal = int.MinValue;
			int bestMove = -1;

			List<int> moves = getMoves(cols);
			foreach (int column in moves)
			{
				playMove(column, "2");
				int value = minimax(0, 4, false);
				if (value > bestVal)
				{
					bestVal = value;
					bestMove = column;
				}
				reverseMove(column);
			}
			return bestMove;
		}

		int minimax(int depth, int maxDepth, bool isMaximizing)
		{
			List<int> moves = getMoves(colPointers);

			if (moves.Count == 0 || depth == maxDepth)
				return evaluateState(state);

			if (isMaximizing)
			{
				int bestVal = int.MinValue;
				foreach (int column in moves)
				{
					playMove(column, "2");
					int value = minimax(depth + 1, maxDepth, !isMaximizing);
					bestVal = Mathf.Max(bestVal, value);
					reverseMove(column);
				}
				return bestVal;
			}

			else
			{
				int bestVal = int.MaxValue;
				foreach (int column in moves)
				{
					playMove(column, "1");
					int value = minimax(depth + 1, maxDepth, !isMaximizing);
					bestVal = Mathf.Min(bestVal, value);
					reverseMove(column);
				}
				return bestVal;
			}

		}

		/// <summary>
		/// Gets all the possible moves.
		/// </summary>
		/// <returns>The possible moves.</returns>
		public List<int> GetPossibleMoves()
		{
			List<int> possibleMoves = new List<int>();
			for (int x = 0; x < numColumns; x++)
			{
				if (field[x, 0] == 0)
				{
					possibleMoves.Add(x);
				}
			}
			return possibleMoves;
		}

		/// <summary>
		/// Spawns a piece at mouse position above the first row
		/// </summary>
		/// <returns>The piece.</returns>
		(GameObject, int, GameObject) SpawnPiece()
		{
			Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int prob = 0;

			if (isPlayersTurn)
			{
				int i = Random.Range(0, blueProbs.Keys.Count);
				List<int> keyList = new List<int>(blueProbs.Keys);
				prob = keyList[i];
				int freq = blueProbs[prob];

				// delete probability from player's list
				blueProbs[prob] -= 1;

				if (prob == 100)
				{
					pieceSuperposition = pieceBlue;
					pieceBlue100Text.GetComponent<TextMesh>().text = blueProbs[100].ToString();
				}
				else if (prob == 75)
				{
					pieceSuperposition = piece75;
					pieceBlue75Text.GetComponent<TextMesh>().text = blueProbs[75].ToString();
				}
				else if (prob == 50)
				{
					pieceSuperposition = piece50;
					pieceBlue50Text.GetComponent<TextMesh>().text = blueProbs[50].ToString();
				}
				else
				{
					pieceSuperposition = piece25;
					pieceBlue25Text.GetComponent<TextMesh>().text = blueProbs[25].ToString();
				}

				if (blueProbs[prob] == 0)
				{
					blueProbs.Remove(prob);
				}

				probText.GetComponent<TextMesh>().text = prob.ToString() + "% YELLOW";
			}
			else
			{
				int i = Random.Range(0, redProbs.Keys.Count);
				List<int> keyList = new List<int>(redProbs.Keys);
				prob = keyList[i];
				int freq = redProbs[prob];

				// delete probability from player's list
				redProbs[prob] -= 1;

				if (prob == 100)
				{
					pieceSuperposition = pieceRed;
					pieceRed100Text.GetComponent<TextMesh>().text = redProbs[100].ToString();
				}
				else if (prob == 75)
				{
					pieceSuperposition = piece25;
					pieceRed75Text.GetComponent<TextMesh>().text = redProbs[75].ToString();
				}
				else if (prob == 50)
				{
					pieceSuperposition = piece50;
					pieceRed50Text.GetComponent<TextMesh>().text = redProbs[50].ToString();
				}
				else
				{
					pieceSuperposition = piece75;
					pieceRed25Text.GetComponent<TextMesh>().text = redProbs[25].ToString();
				}

				if (redProbs[prob] == 0)
				{
					redProbs.Remove(prob);
				}

				probText.GetComponent<TextMesh>().text = prob.ToString() + "% RED";
			}

			List<int> moves = GetPossibleMoves();

			if (moves.Count > 0)
			{
				int column = findBestMove(state, colPointers);

				spawnPos = new Vector3(column, 0, 0);
			}

			GameObject g = Instantiate(pieceSuperposition,
					new Vector3(
					Mathf.Clamp(spawnPos.x, 0, numColumns - 1),
					gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
					Quaternion.identity) as GameObject;

			probText.transform.position = new Vector3(spawnPos.x + 1, gameObjectField.transform.position.y + 1, 0);
			probText.transform.parent = g.transform;

			probText.SetActive(true);

			return (g, prob, probText);
		}

		// Update is called once per frame
		void Update()
		{
			if (isLoading)
				return;

			if (isCheckingForWinner)
				return;

			if (gameOver)
			{
				winningText.SetActive(true);
				btnPlayAgain.SetActive(true);

				playerTurnText.SetActive(false);
				playerTurnObject.SetActive(false);

				// fix play again button
				btnPlayAgain.transform.position = new Vector3(
	(numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) - 1, btnPlayAgain.transform.position.z);
				btnPlayAgain.GetComponent<TextMesh>().color = Color.white;
				btnPlayAgain.GetComponent<TextMesh>().text = "EXIT TO MENU";
				btnPlayAgain.GetComponent<TextMesh>().fontSize = 70;

				UpdatePlayAgainButton();

				return;
			}

			UpdatePlayAgainButton();

			if (isPlayersTurn)
			{
				if (gameObjectTurn == null)
				{
					(gameObjectTurn, probability, probText) = SpawnPiece();
				}
				else
				{
					// update the objects position
					Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					gameObjectTurn.transform.position = new Vector3(
						Mathf.Clamp(pos.x, 0, numColumns - 1),
						gameObjectField.transform.position.y + 1, 0);

					// click the left mouse button to drop the piece into the selected column
					if (Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
					{
						mouseButtonPressed = true;

						probText.transform.parent = null;
						probText.SetActive(false);

						StartCoroutine(dropPiece(gameObjectTurn, probText, probability));
					}
					else
					{
						mouseButtonPressed = false;
					}
				}
			}
			else
			{
				if (gameObjectTurn == null)
				{
					(gameObjectTurn, probability, probText) = SpawnPiece();
				}
				else
				{
					if (!isDropping)
					{
						probText.transform.parent = null;
						probText.SetActive(false);
						//Task.Delay(2000);
						Thread.Sleep(1000);
						StartCoroutine(dropPiece(gameObjectTurn, probText, probability));
					}
				}
			}
		}

		/// <summary>
		/// This method searches for a empty cell and lets 
		/// the object fall down into this cell
		/// </summary>
		/// <param name="gObject">Game Object.</param>
		IEnumerator dropPiece(GameObject gObject, GameObject probText, int probability)
		{

			isDropping = true;

			Vector3 startPosition = gObject.transform.position;
			Vector3 endPosition = new Vector3();

			// round to a grid cell
			int x = Mathf.RoundToInt(startPosition.x);
			startPosition = new Vector3(x, startPosition.y, startPosition.z);

			// is there a free cell in the selected column?
			bool foundFreeCell = false;
			(int, int) tempLocation = (-1, -1);

			for (int i = numRows - 1; i >= 0; i--)
			{
				if (field[x, i] == 0)
				{
					foundFreeCell = true;
					if (isPlayersTurn)
					{
						int p = Random.Range(1, 101);
						if (p < probability)
						{
							Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
							finalColor = Instantiate(
								pieceBlue, // is players turn = spawn blue, else spawn red
								new Vector3(Mathf.Clamp(pos.x, 0, numColumns - 1),
								gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
								Quaternion.identity) as GameObject;
							field[x, i] = (int)Piece.Blue;
							playMove(x, "1");
						}
						else
						{
							Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
							finalColor = Instantiate(
								pieceRed, // is players turn = spawn blue, else spawn red
								new Vector3(Mathf.Clamp(pos.x, 0, numColumns - 1),
								gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
								Quaternion.identity) as GameObject;
							field[x, i] = (int)Piece.Red;
							playMove(x, "2");
						}
					}
					else
					{
						int p = Random.Range(1, 101);
						if (p < probability)
						{
							Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
							finalColor = Instantiate(
								pieceRed, // is players turn = spawn blue, else spawn red
								new Vector3(Mathf.Clamp(pos.x, 0, numColumns - 1),
								gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
								Quaternion.identity) as GameObject;
							field[x, i] = (int)Piece.Red;
							playMove(x, "2");
						}
						else
						{
							Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
							finalColor = Instantiate(
								pieceBlue, // is players turn = spawn blue, else spawn red
								new Vector3(Mathf.Clamp(pos.x, 0, numColumns - 1),
								gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
								Quaternion.identity) as GameObject;
							field[x, i] = (int)Piece.Blue;
							playMove(x, "1");
						}
					}

					endPosition = new Vector3(x, i * -1, startPosition.z);

					break;
				}
			}

			if (foundFreeCell)
			{
				// Instantiate a new Piece, disable the temporary
				GameObject g = Instantiate(finalColor) as GameObject;
				gameObjectTurn.GetComponent<Renderer>().enabled = false;
				finalColor.GetComponent<Renderer>().enabled = false;

				//GameObject p = Instantiate(probText) as GameObject;
				//p.transform.parent = g.transform;
				//p.SetActive(true);

				float distance = Vector3.Distance(startPosition, endPosition);

				float t = 0;
				while (t < 1)
				{
					t += Time.deltaTime * dropTime * ((numRows - distance) + 1);

					g.transform.position = Vector3.Lerp(startPosition, endPosition, t);
					yield return null;
				}

				g.transform.parent = gameObjectField.transform;

				//if (isPlayersTurn)
				//	probDict.Add(g.transform.GetInstanceID(), (probability, tempLocation));
				//else
				//	probDict.Add(g.transform.GetInstanceID(), (100 - probability, tempLocation));

				// remove the temporary gameobject
				DestroyImmediate(gameObjectTurn);

				// run coroutine to check if someone has won
				StartCoroutine(Won());

				// wait until winning check is done
				while (isCheckingForWinner)
					yield return null;

				isPlayersTurn = !isPlayersTurn;
				playerTurnText.GetComponent<TextMesh>().text = isPlayersTurn ? "Yellow's Turn" : "Red's Turn";

				DestroyImmediate(playerTurnObject);

				if (isPlayersTurn)
				{
					playerTurnObject = Instantiate(pieceBlue, new Vector3(numColumns - 1.75f, -6, 1), Quaternion.identity) as GameObject;
					playerTurnObject.transform.localScale -= new Vector3(0.5f, 0.5f, 0);
				}
				else
				{
					playerTurnObject = Instantiate(pieceRed, new Vector3(numColumns - 2.25f, -6, 1), Quaternion.identity) as GameObject;
					playerTurnObject.transform.localScale -= new Vector3(0.5f, 0.5f, 0);

				}
			}

			isDropping = false;

			yield return 0;
		}

		/// <summary>
		/// Check for Winner
		/// </summary>
		IEnumerator Won()
		{
			isCheckingForWinner = true;

			bool blueWon = false;

			for (int x = 0; x < numColumns; x++)
			{
				for (int y = 0; y < numRows; y++)
				{
					//if somebody won, gameOver = true;
					int color = field[x, y];
					if (color != 0)
					{
						//check up
						if (y >= 3 && field[x, y - 1] == color && field[x, y - 2] == color && field[x, y - 3] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}

						//check down
						if (y <= numRows - 4 && field[x, y + 1] == color && field[x, y + 2] == color && field[x, y + 3] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}

						//check left
						if (x >= 3 && field[x - 1, y] == color && field[x - 2, y] == color && field[x - 3, y] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}

						//check right
						if (x <= numColumns - 4 && field[x + 1, y] == color && field[x + 2, y] == color && field[x + 3, y] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}

						//check upper left diagonal
						if (y >= 3 && x >= 3 && field[x - 1, y - 1] == color && field[x - 2, y - 2] == color && field[x - 3, y - 3] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}

						// check upper right diagonal
						if (y >= 3 && x <= numColumns - 4 && field[x + 1, y - 1] == color && field[x + 2, y - 2] == color && field[x + 3, y - 3] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}

						// check lower left diagonal
						if (x >= 3 && y <= numRows - 4 && field[x - 1, y + 1] == color && field[x - 2, y + 2] == color && field[x - 3, y + 3] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}

						//check lower right diagonal
						if (x <= numColumns - 4 && y <= numRows - 4 && field[x + 1, y + 1] == color && field[x + 2, y + 2] == color && field[x + 3, y + 3] == color)
						{
							if (color == 1)
								blueWon = true;
							gameOver = true;
						}
					}
					yield return null;
				}

				yield return null;
			}

			// if Game Over update the winning text to show who has won
			if (gameOver == true)
			{
				winningText.GetComponent<TextMesh>().text = blueWon ? playerWonText : playerLoseText;
			}
			else
			{
				// check if there are any empty cells left, if not set game over and update text to show a draw
				if (!FieldContainsEmptyCell())
				{
					gameOver = true;
					winningText.GetComponent<TextMesh>().text = drawText;
				}
			}

			isCheckingForWinner = false;

			yield return 0;
		}

		void UpdatePlayAgainButton()
		{
			RaycastHit hit;
			//ray shooting out of the camera from where the mouse is
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					btnPlayAgainTouching = true;

					//CreateField();
					Application.LoadLevel(0);
				}
			}
			else
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}

			if (Input.touchCount == 0)
			{
				btnPlayAgainTouching = false;
			}
		}

		/// <summary>
		/// check if the field contains an empty cell
		/// </summary>
		/// <returns><c>true</c>, if it contains empty cell, <c>false</c> otherwise.</returns>
		bool FieldContainsEmptyCell()
		{
			for (int x = 0; x < numColumns; x++)
			{
				for (int y = 0; y < numRows; y++)
				{
					if (field[x, y] == (int)Piece.Empty)
						return true;
				}
			}
			return false;
		}
	}
}
