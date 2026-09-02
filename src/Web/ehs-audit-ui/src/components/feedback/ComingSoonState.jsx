import ConstructionOutlinedIcon from '@mui/icons-material/ConstructionOutlined';
import { EmptyState } from '@/components/feedback/EmptyState';
/** Used only where the owning backend service has no controllers yet (ActionPlan/Reporting — CLAUDE.md phase 5). */
export function ComingSoonState({ feature }) {
    return (<EmptyState icon={<ConstructionOutlinedIcon fontSize="inherit" aria-hidden="true"/>} title={`${feature} is not available yet`} description="This area is wired up in the UI but its backend service hasn't shipped its API yet. Check back once that service is deployed."/>);
}
