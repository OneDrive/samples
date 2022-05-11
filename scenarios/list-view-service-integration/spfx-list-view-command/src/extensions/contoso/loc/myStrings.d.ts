declare interface IContosoCommandSetStrings {
  Command1: string;
  Command2: string;
}

declare module 'ContosoCommandSetStrings' {
  const strings: IContosoCommandSetStrings;
  export = strings;
}
