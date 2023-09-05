export const state = () => ({
  projectsState: [
    {
      id: 1,
      name: 'Project A',
      description: 'This is a description for Project A.',
      deadline: '2023-10-01',
      status: 'In Progress',
      teamMembers: ['Alice', 'Bob']
    },
    {
      id: 2,
      name: 'Project B',
      description: 'This is a description for Project B.',
      deadline: '2023-11-15',
      status: 'Completed',
      teamMembers: ['Charlie', 'David']
    },
    {
      id: 3,
      name: 'Project C',
      description: 'This is a description for Project C.',
      deadline: '2023-09-20',
      status: 'Not Started',
      teamMembers: ['Eve', 'Frank']
    }
  ]
})
