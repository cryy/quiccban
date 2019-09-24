import { ICase } from "../entities/guild/ICase";
import { HookProps } from "./HookProps";

export interface CasePreviewProps extends HookProps {
    case: ICase;
    divider: boolean;
    classes: any;
}