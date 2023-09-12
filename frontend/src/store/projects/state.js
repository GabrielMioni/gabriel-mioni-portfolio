import reactCheckers from '@/assets/react-checkers.png'

export const state = () => ({
  projectsState: [
    {
      id: 1,
      name: 'React Checkers',
      description: 'This 2-player checkers game, built in React.js, offers a mobile-friendly design and a board that maintains a state history, allowing players to undo moves for a seamless gaming experience. The rules deviate slightly from traditional tournament checkers; you aren\'t obligated to make jumps, giving you more strategic freedom.\n\nThe game\'s logic resides primarily in the ReactCheckers component, where the board and its pieces are represented as objects. As players make their moves, the board state is recorded, making it easy to undo actions. Victory conditions are checked at the end of each turn, giving you a game that is as challenging as it is enjoyable.',
      image: reactCheckers,
      git: 'https://github.com/GabrielMioni/react-checkers'
    },
    {
      id: 2,
      name: 'Project B',
      description: 'This is a description for Project B.',
      image: null,
      git: null
    },
    {
      id: 3,
      name: 'Project C',
      description: 'This is a description for Project C.',
      image: null,
      git: null
    }
  ]
})
