# Water Sort Puzzle: Simple Command-Line Version

game_board = {
    '1': [1, 1, 2, 3],
    '2': [1, 2, 2, 3],
    '3': [1, 2, 0, 0],
    '4': [3, 0, 0, 0],
    '5': [3, 0, 0, 0]
}

def printGameBoard():
    for key, bottle in game_board.items():
        print(key + ": " + ''.join([str(color) if color != 0 else ' ' for color in bottle]))

def can_pour(src_bottle, dest_bottle):
    src_color = next((color for color in reversed(src_bottle) if color != 0), 0)
    dest_color = next((color for color in dest_bottle if color != 0), 0)
    
    return src_color != 0 and (dest_color == 0 or dest_color == src_color)

def pour(src_bottle, dest_bottle):
    while can_pour(src_bottle, dest_bottle):
        src_index = next(i for i, color in reversed(list(enumerate(src_bottle))) if color != 0)
        dest_index = next((i for i, color in enumerate(dest_bottle) if color == 0), -1)
        if dest_index == -1: break

        dest_bottle[dest_index] = src_bottle[src_index]
        src_bottle[src_index] = 0

def game_over():
    for _, bottle in game_board.items():
        color = bottle[0]
        if color == 0:
            continue
        for col in bottle:
            if col != 0 and col != color:
                return False
        # Checking if the color is present in any other bottle
        for _, other_bottle in game_board.items():
            if other_bottle is not bottle and color in other_bottle:
                return False
    return True

def no_valid_moves():
    for src_key, src_bottle in game_board.items():
        for dest_key, dest_bottle in game_board.items():
            if src_key != dest_key and can_pour(src_bottle, dest_bottle):
                return False
    return True

def play():
    while not game_over() and not no_valid_moves():
        printGameBoard()
        print("\nChoose a source bottle:")
        src = input()
        print("Choose a destination bottle:")
        dest = input()

        if can_pour(game_board[src], game_board[dest]):
            pour(game_board[src], game_board[dest])
        else:
            print("\nInvalid move. Try again.")

    if game_over():
        printGameBoard()
        print("\nCongratulations! You sorted all bottles!")
    else:
        print("\nNo more valid moves! Try again.")

    restart = input("Do you want to play again? (y/n): ")
    if restart.lower() == "y":
        # When restarting, you'll need to reset the game board to its initial state or another desired state.
        game_board.update({
            '1': [1, 1, 2, 3],
            '2': [1, 2, 2, 3],
            '3': [1, 2, 0, 0],
            '4': [3, 0, 0, 0],
            '5': [3, 0, 0, 0]
        })
        play()

if __name__ == "__main__":
    play()
