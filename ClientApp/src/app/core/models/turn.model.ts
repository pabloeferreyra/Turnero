export interface TurnDto {
  id: string;
  name: string;
  dni: string;
  medicId: string;
  medicName?: string;
  date: string;
  time: string;
  timeId: string;
  socialWork?: string;
  reason?: string;
  accessed: boolean;
}

export interface TurnCreateRequest {
  name: string;
  dni: string;
  medicId: string;
  dateTurn: string;
  timeId: string;
  socialWork?: string;
  reason?: string;
}

export interface TurnUpdateRequest extends TurnCreateRequest {
  id: string;
  accessed: boolean;
}
