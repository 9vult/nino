
export const interp = (input: string, swaps: {[key:string]:any}) => {
  for (let key of Object.keys(swaps)) {
    input = input.replaceAll(key, swaps[key]);
  }
  return input;
};